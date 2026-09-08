import { Component, inject, OnInit, signal, computed, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslocoModule, TranslocoService } from '@jsverse/transloco';
import { PomodoroService } from '../../core/services/pomodoro.service';
import { UserSettingsService } from '../../core/services/user-settings.service';
import { TimeService } from '../../core/services/time.service';
import { PomodoroSessionType } from '../../shared/models/pomodoro.model';

@Component({
  selector: 'app-pomodoro',
  standalone: true,
  imports: [CommonModule, TranslocoModule],
  templateUrl: './pomodoro.component.html',
  styleUrl: './pomodoro.component.scss'
})
export class PomodoroComponent implements OnInit {
  private pomodoroService = inject(PomodoroService);
  private settingsService = inject(UserSettingsService);
  private timeService = inject(TimeService);
  private transloco = inject(TranslocoService);

  PomodoroSessionType = PomodoroSessionType;

  active = this.pomodoroService.active;
  todayCompleted = this.pomodoroService.todayCompleted;
  settings = this.settingsService.settings;
  now = this.timeService.now;

  nextType = signal<PomodoroSessionType>(PomodoroSessionType.Work);
  isFinished = signal(false);

  completedWorkRounds = computed(() =>
    this.todayCompleted().filter(s => s.type === PomodoroSessionType.Work).length
  );

  roundsBeforeLongBreak = computed(() => this.settings()?.pomodoroRoundsBeforeLongBreak ?? 4);

  currentRoundInCycle = computed(() => {
    const rounds = this.roundsBeforeLongBreak();
    return rounds === 0 ? 0 : this.completedWorkRounds() % rounds;
  });

  durationMinutes = computed(() => {
    const s = this.settings();
    if (!s) return 25;

    const type = this.active()?.type ?? this.nextType();
    switch (type) {
      case PomodoroSessionType.ShortBreak: return s.pomodoroShortBreakMinutes;
      case PomodoroSessionType.LongBreak: return s.pomodoroLongBreakMinutes;
      default: return s.pomodoroWorkMinutes;
    }
  });

  remainingSeconds = computed(() => {
    const session = this.active();
    if (!session) return this.durationMinutes() * 60;

    const start = new Date(session.startedAt).getTime();
    const current = this.now().getTime();
    const totalSeconds = this.durationMinutes() * 60;
    const elapsedSeconds = Math.floor((current - start) / 1000);
    return Math.max(0, totalSeconds - elapsedSeconds);
  });

  displayMinutes = computed(() => Math.floor(this.remainingSeconds() / 60));
  displaySeconds = computed(() => this.remainingSeconds() % 60);

  progressPercent = computed(() => {
    const total = this.durationMinutes() * 60;
    if (total === 0) return 0;
    return (this.remainingSeconds() / total) * 100;
  });

  isBreak = computed(() => {
    const type = this.active()?.type ?? this.nextType();
    return type === PomodoroSessionType.ShortBreak || type === PomodoroSessionType.LongBreak;
  });

  phaseLabel = computed(() => {
    const type = this.active()?.type ?? this.nextType();
    switch (type) {
      case PomodoroSessionType.ShortBreak: return this.transloco.translate('pomodoro.shortBreak');
      case PomodoroSessionType.LongBreak: return this.transloco.translate('pomodoro.longBreak');
      default: return this.transloco.translate('pomodoro.focus');
    }
  });

  constructor() {
    effect(() => {
      this.now();
      this.onTick();
    });
  }

  async ngOnInit() {
    if (!this.settings()) await this.settingsService.load();
    await this.pomodoroService.loadToday();
    await this.pomodoroService.loadActive();
  }

  async start() {
    const type = this.active()?.type ?? this.nextType();
    this.isFinished.set(false);
    await this.pomodoroService.start(type);
  }

  async onTick() {
    const session = this.active();
    if (!session || this.isFinished()) return;

    if (this.remainingSeconds() === 0) {
      this.isFinished.set(true);
      await this.pomodoroService.complete(session.id);

      const finishedType = session.type;
      if (finishedType === PomodoroSessionType.Work) {
        const isLongBreak = this.roundsBeforeLongBreak() > 0
          && this.completedWorkRounds() % this.roundsBeforeLongBreak() === 0;
        this.nextType.set(isLongBreak ? PomodoroSessionType.LongBreak : PomodoroSessionType.ShortBreak);
      } else {
        this.nextType.set(PomodoroSessionType.Work);
      }
      this.isFinished.set(false);
    }
  }

  async reset() {
    const session = this.active();
    if (!session) return;
    await this.pomodoroService.reset(session.id);
  }
}

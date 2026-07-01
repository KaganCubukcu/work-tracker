import { Component, inject, computed, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { WorkSessionService } from '../../core/services/work-session.service';
import { TimeService } from '../../core/services/time.service';

@Component({
  selector: 'app-time-tracker',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './time-tracker.component.html',
  styleUrl: './time-tracker.component.scss'
})
export class TimeTrackerComponent implements OnInit {
  private sessionService = inject(WorkSessionService);
  private timeService = inject(TimeService);

  session = this.sessionService.session;
  now = this.timeService.now;
  showEdit = signal(false);

  localStartTime = computed(() => {
    const s = this.session();
    return s ? this.timeService.toLocalTimeString(s.startTime) : '';
  });

  elapsed = computed(() => {
    const s = this.session();
    if (!s) return { hours: 0, minutes: 0, seconds: 0 };

    const start = new Date(s.startTime).getTime();
    const current = this.now().getTime();
    const diffMs = Math.max(0, current - start);

    return this.msToHms(diffMs);
  });

  remaining = computed(() => {
    const s = this.session();
    if (!s) return { hours: 0, minutes: 0, seconds: 0, isOvertime: false };

    const start = new Date(s.startTime).getTime();
    const current = this.now().getTime();
    const expectedMs = s.expectedDailyHours * 60 * 60 * 1000;
    const elapsedMs = current - start;
    const remainingMs = expectedMs - elapsedMs;

    return {
      ...this.msToHms(Math.abs(remainingMs)),
      isOvertime: remainingMs < 0
    };
  });

  // Mesainin yüzde kaçı tükendi (bar dolum oranı)
  progressPercent = computed(() => {
    const s = this.session();
    if (!s) return 0;

    const start = new Date(s.startTime).getTime();
    const current = this.now().getTime();
    const expectedMs = s.expectedDailyHours * 60 * 60 * 1000;
    const elapsedMs = current - start;

    return Math.min(100, Math.max(0, (elapsedMs / expectedMs) * 100));
  });

  ngOnInit() {
    this.sessionService.loadToday();
  }

  private msToHms(ms: number) {
    const totalSeconds = Math.floor(ms / 1000);
    const hours = Math.floor(totalSeconds / 3600);
    const minutes = Math.floor((totalSeconds % 3600) / 60);
    const seconds = totalSeconds % 60;
    return { hours, minutes, seconds };
  }

  editStartTime(newLocalTime: string) {
    const s = this.session();
    if (!s) return;
    const utcIso = this.timeService.localTimeToUtcIso(newLocalTime, s.startTime);
    this.sessionService.update(utcIso, s.expectedDailyHours);
  }

  editExpectedHours(newHours: number) {
    const s = this.session();
    if (!s) return;
    this.sessionService.update(s.startTime, newHours);
  }
}
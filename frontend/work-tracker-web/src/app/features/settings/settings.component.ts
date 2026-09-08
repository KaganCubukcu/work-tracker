import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslocoModule } from '@jsverse/transloco';
import { BreakSlotService } from '../../core/services/break-slot.service';
import { UserSettingsService } from '../../core/services/user-settings.service';
import { BreakSlot } from '../../shared/models/break-slot.model';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [RouterLink, TranslocoModule],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.scss'
})
export class SettingsComponent implements OnInit {
  private breakService = inject(BreakSlotService);
  private userSettingsService = inject(UserSettingsService);

  breaks = this.breakService.breaks;
  userSettings = this.userSettingsService.settings;

  newLabel = signal('');
  newStart = signal('');
  newEnd = signal('');

  editingId = signal<string | null>(null);
  editLabel = signal('');
  editStart = signal('');
  editEnd = signal('');

  pomodoroWorkMinutes = signal(25);
  pomodoroShortBreakMinutes = signal(5);
  pomodoroLongBreakMinutes = signal(15);
  pomodoroRoundsBeforeLongBreak = signal(4);

  async ngOnInit() {
    this.breakService.load();
    await this.userSettingsService.load();

    const s = this.userSettings();
    if (s) {
      this.pomodoroWorkMinutes.set(s.pomodoroWorkMinutes);
      this.pomodoroShortBreakMinutes.set(s.pomodoroShortBreakMinutes);
      this.pomodoroLongBreakMinutes.set(s.pomodoroLongBreakMinutes);
      this.pomodoroRoundsBeforeLongBreak.set(s.pomodoroRoundsBeforeLongBreak);
    }
  }

  savePomodoroSettings() {
    this.userSettingsService.updatePomodoroSettings({
      pomodoroWorkMinutes: this.pomodoroWorkMinutes(),
      pomodoroShortBreakMinutes: this.pomodoroShortBreakMinutes(),
      pomodoroLongBreakMinutes: this.pomodoroLongBreakMinutes(),
      pomodoroRoundsBeforeLongBreak: this.pomodoroRoundsBeforeLongBreak(),
    });
  }

  addBreak() {
    const label = this.newLabel().trim();
    const start = this.newStart();
    const end = this.newEnd();

    if (!label || !start || !end) return;

    this.breakService.add(label, `${start}:00`, `${end}:00`);

    this.newLabel.set('');
    this.newStart.set('');
    this.newEnd.set('');
  }

  startEdit(slot: BreakSlot) {
    this.editingId.set(slot.id);
    this.editLabel.set(slot.label);
    this.editStart.set(slot.startTime.slice(0, 5));
    this.editEnd.set(slot.endTime.slice(0, 5));
  }

  cancelEdit() {
    this.editingId.set(null);
  }

  saveEdit(id: string) {
    const label = this.editLabel().trim();
    const start = this.editStart();
    const end = this.editEnd();

    if (!label || !start || !end) return;

    this.breakService.update(id, label, `${start}:00`, `${end}:00`);
    this.editingId.set(null);
  }

  removeBreak(id: string) {
    this.breakService.remove(id);
  }
}
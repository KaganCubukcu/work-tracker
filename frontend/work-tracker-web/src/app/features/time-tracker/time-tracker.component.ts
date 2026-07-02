import { Component, inject, computed, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { WorkSessionService } from '../../core/services/work-session.service';
import { TimeService } from '../../core/services/time.service';
import { BreakSlotService } from '../../core/services/break-slot.service';

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
  private breakService = inject(BreakSlotService);

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

  // Şu ana kadar geçmiş molaların toplam süresi (ms)
  private elapsedBreakMs = computed(() => {
    const currentTime = this.now();
    const breaks = this.breakService.breaks();

    return breaks.reduce((total, b) => {
      const start = this.toTodayDate(b.startTime);
      const end = this.toTodayDate(b.endTime);

      if (currentTime >= end) {
        return total + (end.getTime() - start.getTime());
      } else if (currentTime > start && currentTime < end) {
        return total + (currentTime.getTime() - start.getTime());
      }
      return total;
    }, 0);
  });

  remaining = computed(() => {
    const s = this.session();
    if (!s) return { hours: 0, minutes: 0, seconds: 0, isOvertime: false };

    const start = new Date(s.startTime).getTime();
    const current = this.now().getTime();
    const expectedMs = s.expectedDailyHours * 60 * 60 * 1000;
    const rawElapsedMs = current - start;
    const netElapsedMs = Math.max(0, rawElapsedMs - this.elapsedBreakMs());
    const remainingMs = expectedMs - netElapsedMs;

    return {
      ...this.msToHms(Math.abs(remainingMs)),
      isOvertime: remainingMs < 0
    };
  });

  progressPercent = computed(() => {
      const s = this.session();
      if (!s) return 0;

      const start = new Date(s.startTime).getTime();
      const current = this.now().getTime();
      const expectedMs = s.expectedDailyHours * 60 * 60 * 1000;
      const rawElapsedMs = current - start;
      const netElapsedMs = Math.max(0, rawElapsedMs - this.elapsedBreakMs());

      return Math.min(100, Math.max(0, (netElapsedMs / expectedMs) * 100));
    });

   breakStatuses = computed(() => {
    const currentTime = this.now();
    const breaks = this.breakService.breaks();

    return breaks.map(b => {
      const start = this.toTodayDate(b.startTime);
      const end = this.toTodayDate(b.endTime);

      let status: 'upcoming' | 'active' | 'passed';
      let countdownMs = 0;

      if (currentTime < start) {
        status = 'upcoming';
        countdownMs = start.getTime() - currentTime.getTime();
      } else if (currentTime >= start && currentTime <= end) {
        status = 'active';
        countdownMs = end.getTime() - currentTime.getTime();
      } else {
        status = 'passed';
      }

      return {
        ...b,
        status,
        countdown: this.msToShortHm(countdownMs)
      };
    });
  });

  ngOnInit() {
    this.sessionService.loadToday();
      this.breakService.load();
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

  private toTodayDate(timeOnlyString: string): Date {
    const [hours, minutes] = timeOnlyString.split(':').map(Number);
    const now = new Date();
    return new Date(now.getFullYear(), now.getMonth(), now.getDate(), hours, minutes, 0);
  }

  private msToShortHm(ms: number): string {
    const totalMinutes = Math.floor(ms / 60000);
    const hours = Math.floor(totalMinutes / 60);
    const minutes = totalMinutes % 60;
    return hours > 0 ? `${hours}sa ${minutes}dk` : `${minutes}dk`;
  }
}
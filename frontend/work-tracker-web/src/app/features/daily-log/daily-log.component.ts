import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { DailyLogService } from '../../core/services/daily-log.service';
import { TimeService } from '../../core/services/time.service';

@Component({
  selector: 'app-daily-log',
  standalone: true,
  templateUrl: './daily-log.component.html',
  styleUrl: './daily-log.component.scss'
})
export class DailyLogComponent implements OnInit {
  private logService = inject(DailyLogService);
  private timeService = inject(TimeService);

  logs = this.logService.logs;
  newEntry = signal('');

  ngOnInit() {
    this.logService.loadToday();
  }

  addEntry() {
    const content = this.newEntry().trim();
    if (!content) return;

    this.logService.add(content);
    this.newEntry.set('');
  }

  removeEntry(id: number) {
    this.logService.remove(id);
  }

  formatTime(createdAt: string): string {
    return this.timeService.toLocalTimeString(createdAt);
  }
}
import { Component, inject, OnInit, signal, computed } from '@angular/core';
import { TranslocoModule } from '@jsverse/transloco';
import { DailyLogService } from '../../core/services/daily-log.service';
import { TimeService } from '../../core/services/time.service';
import { DailyLog } from '../../shared/models/daily-log.model';

@Component({
  selector: 'app-daily-log',
  standalone: true,
  imports: [TranslocoModule],
  templateUrl: './daily-log.component.html',
  styleUrl: './daily-log.component.scss'
})
export class DailyLogComponent implements OnInit {
  private logService = inject(DailyLogService);
  private timeService = inject(TimeService);

  logs = this.logService.logs;
  newEntry = signal('');

  searchInput = signal('');
  dateFrom = signal('');
  dateTo = signal('');
  searchResults = this.logService.searchResults;

  editingId = signal<number | null>(null);
  editContent = signal('');

  isFiltering = computed(() =>
    this.searchInput().trim() !== '' || this.dateFrom() !== '' || this.dateTo() !== ''
  );

  displayedLogs = computed(() =>
    this.isFiltering() ? this.searchResults() : this.logs()
  );

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

  startEdit(log: DailyLog) {
    this.editingId.set(log.id);
    this.editContent.set(log.content);
  }

  cancelEdit() {
    this.editingId.set(null);
  }

  async saveEdit(id: number) {
    const content = this.editContent().trim();
    if (!content) return;
    await this.logService.update(id, content);
    this.editingId.set(null);
  }

  onSearchInput(value: string) {
    this.searchInput.set(value);
    this.logService.setSearchQuery(value);
  }

  onDateFromChange(value: string) {
    this.dateFrom.set(value);
    this.applyDateRange();
  }

  onDateToChange(value: string) {
    this.dateTo.set(value);
    this.applyDateRange();
  }

  private applyDateRange() {
    const from = this.dateFrom() ? new Date(this.dateFrom()) : null;
    const to = this.dateTo() ? new Date(this.dateTo()) : null;
    this.logService.setDateRange(from, to);
  }

  clearFilters() {
    this.searchInput.set('');
    this.dateFrom.set('');
    this.dateTo.set('');
    this.logService.clearFilters();
  }

  formatTime(createdAt: string): string {
    return this.timeService.toLocalTimeString(createdAt);
  }
}
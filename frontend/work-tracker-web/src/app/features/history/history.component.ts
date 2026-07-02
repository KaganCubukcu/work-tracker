import { Component, inject, OnInit, computed } from '@angular/core';
import { RouterLink } from '@angular/router';
import { HistoryService } from '../../core/services/history.service';
import { TimeService } from '../../core/services/time.service';

@Component({
  selector: 'app-history',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './history.component.html',
  styleUrl: './history.component.scss'
})
export class HistoryComponent implements OnInit {
  private historyService = inject(HistoryService);
  private timeService = inject(TimeService);

  availableDates = this.historyService.availableDates;
  selectedDay = this.historyService.selectedDay;

  // Bugün hariç geçmiş günler (bugünü zaten dashboard'da görüyorsun)
  pastDates = computed(() => {
    const today = new Date().toISOString().slice(0, 10);
    return this.availableDates().filter(d => d !== today);
  });

  ngOnInit() {
    this.historyService.loadAvailableDates();
  }

  selectDay(date: string) {
    this.historyService.loadDay(date);
  }

  formatDate(dateStr: string): string {
    const date = new Date(dateStr);
    return new Intl.DateTimeFormat('tr-TR', {
      weekday: 'long',
      day: 'numeric',
      month: 'long'
    }).format(date);
  }

  formatTime(isoStr: string): string {
    return this.timeService.toLocalTimeString(isoStr);
  }

  sessionDuration(startTime: string): string {
    return this.formatTime(startTime);
  }
}
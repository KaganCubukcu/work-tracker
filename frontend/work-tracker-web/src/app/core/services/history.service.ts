import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { DayHistory } from '../../shared/models/history.model';

@Injectable({ providedIn: 'root' })
export class HistoryService {
  private http = inject(HttpClient);
  private apiUrl = '/api/history';

  availableDates = signal<string[]>([]);
  selectedDay = signal<DayHistory | null>(null);

  async loadAvailableDates() {
    try {
      const data = await this.http.get<string[]>(this.apiUrl).toPromise();
      this.availableDates.set(data ?? []);
    } catch (err) {
      console.error('Geçmiş tarihler yüklenemedi:', err);
    }
  }

  async loadDay(date: string) {
    try {
      const data = await this.http.get<DayHistory>(`${this.apiUrl}/${date}`).toPromise();
      this.selectedDay.set(data ?? null);
    } catch (err) {
      console.error('Gün detayı yüklenemedi:', err);
    }
  }
}
import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { DailyLog } from '../../shared/models/daily-log.model';

@Injectable({ providedIn: 'root' })
export class DailyLogService {
  private http = inject(HttpClient);
  private apiUrl = '/api/daily-logs';

  logs = signal<DailyLog[]>([]);

  async loadToday() {
    const data = await this.http.get<DailyLog[]>(`${this.apiUrl}/today`).toPromise();
    this.logs.set(data ?? []);
  }

  async add(content: string) {
    const newLog = await this.http
      .post<DailyLog>(this.apiUrl, { content })
      .toPromise();
    if (newLog) {
      this.logs.update(list => [newLog, ...list]);
    }
  }

  async remove(id: number) {
    await this.http.delete(`${this.apiUrl}/${id}`).toPromise();
    this.logs.update(list => list.filter(l => l.id !== id));
  }
}
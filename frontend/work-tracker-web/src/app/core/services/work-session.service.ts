import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { WorkSession } from '../../shared/models/work-session.model';

@Injectable({ providedIn: 'root' })
export class WorkSessionService {
  private http = inject(HttpClient);
  private apiUrl = '/api/work-sessions';

  session = signal<WorkSession | null>(null);

  async loadToday() {
    const data = await this.http.get<WorkSession>(`${this.apiUrl}/today`).toPromise();
    this.session.set(data ?? null);
  }

  async update(startTime: string, expectedDailyHours: number) {
    const current = this.session();
    if (!current) return;

    const updated = await this.http
      .put<WorkSession>(`${this.apiUrl}/${current.id}`, {
        ...current,
        startTime,
        expectedDailyHours
      })
      .toPromise();

    if (updated) this.session.set(updated);
  }
}
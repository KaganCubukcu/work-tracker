import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { PomodoroSession, PomodoroSessionType } from '../../shared/models/pomodoro.model';

@Injectable({ providedIn: 'root' })
export class PomodoroService {
  private http = inject(HttpClient);
  private apiUrl = '/api/pomodoro';

  active = signal<PomodoroSession | null>(null);
  todayCompleted = signal<PomodoroSession[]>([]);

  async loadActive() {
    try {
      const data = await firstValueFrom(this.http.get<PomodoroSession | null>(`${this.apiUrl}/active`));
      this.active.set(data ?? null);
    } catch (err) {
      console.error('Aktif pomodoro yüklenemedi:', err);
    }
  }

  async loadToday() {
    try {
      const data = await firstValueFrom(this.http.get<PomodoroSession[]>(`${this.apiUrl}/today`));
      this.todayCompleted.set(data ?? []);
    } catch (err) {
      console.error('Bugünkü pomodorolar yüklenemedi:', err);
    }
  }

  async start(type: PomodoroSessionType) {
    try {
      const created = await firstValueFrom(
        this.http.post<PomodoroSession>(this.apiUrl, { type }),
      );
      if (created) this.active.set(created);
    } catch (err) {
      console.error('Pomodoro başlatılamadı:', err);
    }
  }

  async complete(id: string) {
    try {
      const completed = await firstValueFrom(
        this.http.put<PomodoroSession>(`${this.apiUrl}/${id}/complete`, {}),
      );
      if (completed) {
        this.active.set(null);
        this.todayCompleted.update(list => [...list, completed]);
      }
    } catch (err) {
      console.error('Pomodoro tamamlanamadı:', err);
    }
  }

  async reset(id: string) {
    try {
      await firstValueFrom(this.http.delete(`${this.apiUrl}/${id}`));
      this.active.set(null);
    } catch (err) {
      console.error('Pomodoro sıfırlanamadı:', err);
    }
  }
}

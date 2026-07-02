import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { UserSettings } from '../../shared/models/user-settings.model';

@Injectable({ providedIn: 'root' })
export class UserSettingsService {
  private http = inject(HttpClient);
  private apiUrl = '/api/settings';

  settings = signal<UserSettings | null>(null);

  async load() {
    try {
      const data = await this.http.get<UserSettings>(this.apiUrl).toPromise();
      this.settings.set(data ?? null);
    } catch (err) {
      console.error('Ayarlar yüklenemedi:', err);
    }
  }

  async updateHireDate(hireDate: string) {
    try {
      const current = this.settings();
      const updated = await this.http
        .put<UserSettings>(this.apiUrl, { ...current, hireDate })
        .toPromise();
      if (updated) this.settings.set(updated);
    } catch (err) {
      console.error('İşe giriş tarihi güncellenemedi:', err);
    }
  }
}
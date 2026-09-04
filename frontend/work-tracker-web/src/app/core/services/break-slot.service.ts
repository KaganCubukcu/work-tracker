import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { BreakSlot } from '../../shared/models/break-slot.model';

@Injectable({ providedIn: 'root' })
export class BreakSlotService {
  private http = inject(HttpClient);
  private apiUrl = '/api/break-slots';

  breaks = signal<BreakSlot[]>([]);

  async load() {
    try {
      const data = await firstValueFrom(this.http.get<BreakSlot[]>(this.apiUrl));
      this.breaks.set(data ?? []);
    } catch (err) {
      console.error('Molalar yüklenemedi:', err);
    }
  }

  async add(label: string, startTime: string, endTime: string) {
    try {
      const newSlot = await firstValueFrom(
        this.http.post<BreakSlot>(this.apiUrl, { label, startTime, endTime }),
      );
      if (newSlot) {
        this.breaks.update(list =>
          [...list, newSlot].sort((a, b) => a.startTime.localeCompare(b.startTime))
        );
      }
    } catch (err) {
      console.error('Mola eklenemedi:', err);
    }
  }

  async update(id: string, label: string, startTime: string, endTime: string) {
    try {
      const updated = await firstValueFrom(
        this.http.put<BreakSlot>(`${this.apiUrl}/${id}`, { label, startTime, endTime }),
      );
      if (updated) {
        this.breaks.update(list =>
          list
            .map(b => (b.id === updated.id ? updated : b))
            .sort((a, b) => a.startTime.localeCompare(b.startTime))
        );
      }
    } catch (err) {
      console.error('Mola güncellenemedi:', err);
    }
  }

  async remove(id: string) {
    try {
      await firstValueFrom(this.http.delete(`${this.apiUrl}/${id}`));
      this.breaks.update(list => list.filter(b => b.id !== id));
    } catch (err) {
      console.error('Mola silinemedi:', err);
    }
  }
}
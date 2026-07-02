import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BreakSlot } from '../../shared/models/break-slot.model';

@Injectable({ providedIn: 'root' })
export class BreakSlotService {
  private http = inject(HttpClient);
  private apiUrl = '/api/break-slots';

  breaks = signal<BreakSlot[]>([]);

  async load() {
    try {
      const data = await this.http.get<BreakSlot[]>(this.apiUrl).toPromise();
      this.breaks.set(data ?? []);
    } catch (err) {
      console.error('Molalar yüklenemedi:', err);
    }
  }
}
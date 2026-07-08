import { Injectable, inject, signal } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { combineLatest } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap } from 'rxjs/operators';
import { DailyLog } from '../../shared/models/daily-log.model';

@Injectable({ providedIn: 'root' })
export class DailyLogService {
  private http = inject(HttpClient);
  private apiUrl = '/api/daily-logs';

  logs = signal<DailyLog[]>([]);
                                                                                                                                                                                                                                                                                                                                                                             
  private searchQuery = signal('');
  private dateFrom = signal<string | null>(null);
  private dateTo = signal<string | null>(null);

  private filters$ = combineLatest([
    toObservable(this.searchQuery).pipe(debounceTime(300), distinctUntilChanged()),
    toObservable(this.dateFrom),
    toObservable(this.dateTo)
  ]).pipe(
    switchMap(([q, from, to]) => {
      let params = new HttpParams();
      if (q) params = params.set('q', q);
      if (from) params = params.set('from', from);
      if (to) params = params.set('to', to);
      return this.http.get<DailyLog[]>(`${this.apiUrl}/search`, { params });
    })
  );

  searchResults = toSignal(this.filters$, { initialValue: [] as DailyLog[] });

  setSearchQuery(q: string) {
    this.searchQuery.set(q);
  }

  setDateRange(from: Date | null, to: Date | null) {
    this.dateFrom.set(from ? this.toLocalDateString(from) : null);
    this.dateTo.set(to ? this.toLocalDateString(to) : null);
  }

  clearFilters() {
    this.searchQuery.set('');
    this.dateFrom.set(null);
    this.dateTo.set(null);
  }

  private toLocalDateString(date: Date): string {
    const y = date.getFullYear();
    const m = String(date.getMonth() + 1).padStart(2, '0');
    const d = String(date.getDate()).padStart(2, '0');
    return `${y}-${m}-${d}`;
  }

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
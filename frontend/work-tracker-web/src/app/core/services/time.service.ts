import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class TimeService {
  now = signal(new Date());

  constructor() {
    setInterval(() => {
      this.now.set(new Date());
    }, 1000);
  }

  toLocalTimeString(utcIsoString: string): string {
    const date = new Date(utcIsoString);
    const hours = date.getHours().toString().padStart(2, '0');
    const minutes = date.getMinutes().toString().padStart(2, '0');
    return `${hours}:${minutes}`;
  }

  localTimeToUtcIso(localTimeHHmm: string, referenceUtcIsoString: string): string {
    const [hours, minutes] = localTimeHHmm.split(':').map(Number);
    const reference = new Date(referenceUtcIsoString);
    const localDate = new Date(
      reference.getFullYear(),
      reference.getMonth(),
      reference.getDate(),
      hours,
      minutes,
      0
    );

    return localDate.toISOString();
  }
}
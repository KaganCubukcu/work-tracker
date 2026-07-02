import { Component, inject, computed, OnInit, signal } from '@angular/core';
import { UserSettingsService } from '../../core/services/user-settings.service';

@Component({
  selector: 'app-tenure-badge',
  standalone: true,
  templateUrl: './tenure-badge.component.html',
  styleUrl: './tenure-badge.component.scss'
})
export class TenureBadgeComponent implements OnInit {
  private settingsService = inject(UserSettingsService);

  settings = this.settingsService.settings;
  showEdit = signal(false);

  dayCount = computed(() => {
    const s = this.settings();
    if (!s?.hireDate) return null;

    const hireDate = new Date(s.hireDate);
    const today = new Date();

    // Sadece takvim günü farkı, saat bilgisini sıfırla
    hireDate.setHours(0, 0, 0, 0);
    today.setHours(0, 0, 0, 0);

    const diffMs = today.getTime() - hireDate.getTime();
    const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24)) + 1; // ilk gün de dahil (1. gün)

    return diffDays;
  });

  ngOnInit() {
    this.settingsService.load();
  }

  saveHireDate(dateStr: string) {
    if (!dateStr) return;
    this.settingsService.updateHireDate(dateStr);
    this.showEdit.set(false);
  }
}
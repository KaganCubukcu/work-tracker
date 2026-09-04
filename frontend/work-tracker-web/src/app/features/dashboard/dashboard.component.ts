import { Component, computed, inject } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { TranslocoModule, TranslocoService } from '@jsverse/transloco';
import { toSignal } from '@angular/core/rxjs-interop';
import { TimeTrackerComponent } from '../time-tracker/time-tracker.component';
import { TodoComponent } from '../todo/todo.component';
import { DailyLogComponent } from '../daily-log/daily-log.component';
import { TenureBadgeComponent } from '../tenure-badge/tenure-badge.component';
import { LangSwitcherComponent } from '../../shared/lang-switcher/lang-switcher.component';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterLink, TranslocoModule, LangSwitcherComponent, TimeTrackerComponent, TodoComponent, DailyLogComponent, TenureBadgeComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent {
  private transloco = inject(TranslocoService);
  private auth = inject(AuthService);
  private router = inject(Router);
  private activeLang = toSignal(this.transloco.langChanges$, { initialValue: this.transloco.getActiveLang() });

  today = computed(() => {
    const locale = this.activeLang() === 'tr' ? 'tr-TR' : 'en-US';
    const formatter = new Intl.DateTimeFormat(locale, {
      weekday: 'long',
      day: 'numeric',
      month: 'long'
    });
    return formatter.format(new Date());
  });

  async logout() {
    await this.auth.logout();
    this.router.navigate(['/login']);
  }
}
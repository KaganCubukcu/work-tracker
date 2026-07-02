import { Component, computed } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TimeTrackerComponent } from '../time-tracker/time-tracker.component';
import { TodoComponent } from '../todo/todo.component';
import { DailyLogComponent } from '../daily-log/daily-log.component';
import { TenureBadgeComponent } from '../tenure-badge/tenure-badge.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [RouterLink, TimeTrackerComponent, TodoComponent, DailyLogComponent, TenureBadgeComponent],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss'
})
export class DashboardComponent {
  today = computed(() => {
    const formatter = new Intl.DateTimeFormat('tr-TR', {
      weekday: 'long',
      day: 'numeric',
      month: 'long'
    });
    return formatter.format(new Date());
  });
}
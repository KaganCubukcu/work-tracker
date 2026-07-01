import { Component, computed } from '@angular/core';
import { TimeTrackerComponent } from '../time-tracker/time-tracker.component';
import { TodoComponent } from '../todo/todo.component';
import { DailyLogComponent } from '../daily-log/daily-log.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [TimeTrackerComponent, TodoComponent, DailyLogComponent],
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
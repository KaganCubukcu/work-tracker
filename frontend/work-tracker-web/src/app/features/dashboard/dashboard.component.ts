import { Component } from '@angular/core';
import { TimeTrackerComponent } from '../time-tracker/time-tracker.component';
import { TodoComponent } from '../todo/todo.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [TimeTrackerComponent, TodoComponent],
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent {}
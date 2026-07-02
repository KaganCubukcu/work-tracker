import { WorkSession } from './work-session.model';
import { TodoItem } from './todo.model';
import { DailyLog } from './daily-log.model';

export interface DayHistory {
  date: string;
  session: WorkSession | null;
  todos: TodoItem[];
  logs: DailyLog[];
}
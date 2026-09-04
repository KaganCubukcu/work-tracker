export interface TodoItem {
  id: string;
  title: string;
  isDone: boolean;
  createdAt: string;
  completedAt: string | null;
}
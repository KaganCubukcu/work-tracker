export interface TodoItem {
  id: number;
  title: string;
  isDone: boolean;
  createdAt: string;
  completedAt: string | null;
}
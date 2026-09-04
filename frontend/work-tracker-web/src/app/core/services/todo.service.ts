import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { TodoItem } from '../../shared/models/todo.model';

@Injectable({ providedIn: 'root' })
export class TodoService {
  private http = inject(HttpClient);
  private apiUrl = '/api/todos';

  todos = signal<TodoItem[]>([]);

  async load() {
    const data = await firstValueFrom(this.http.get<TodoItem[]>(this.apiUrl));
    this.todos.set(data ?? []);
  }

  async add(title: string) {
    const newTodo = await firstValueFrom(
      this.http.post<TodoItem>(this.apiUrl, { title, isDone: false }),
    );
    if (newTodo) {
      this.todos.update(list => [newTodo, ...list]);
    }
  }

  async toggle(todo: TodoItem) {
    const updated = await firstValueFrom(
      this.http.put<TodoItem>(`${this.apiUrl}/${todo.id}`, { ...todo, isDone: !todo.isDone }),
    );
    if (updated) {
      this.todos.update(list =>
        list.map(t => (t.id === updated.id ? updated : t))
      );
    }
  }

  async remove(id: string) {
    await firstValueFrom(this.http.delete(`${this.apiUrl}/${id}`));
    this.todos.update(list => list.filter(t => t.id !== id));
  }
}
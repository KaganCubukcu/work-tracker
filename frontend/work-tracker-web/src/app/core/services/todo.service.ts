import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { TodoItem } from '../../shared/models/todo.model';

@Injectable({ providedIn: 'root' })
export class TodoService {
  private http = inject(HttpClient);
  private apiUrl = '/api/todos';

  todos = signal<TodoItem[]>([]);

  async load() {
    const data = await this.http.get<TodoItem[]>(this.apiUrl).toPromise();
    this.todos.set(data ?? []);
  }

  async add(title: string) {
    const newTodo = await this.http
      .post<TodoItem>(this.apiUrl, { title, isDone: false })
      .toPromise();
    if (newTodo) {
      this.todos.update(list => [newTodo, ...list]);
    }
  }

  async toggle(todo: TodoItem) {
    const updated = await this.http
      .put<TodoItem>(`${this.apiUrl}/${todo.id}`, { ...todo, isDone: !todo.isDone })
      .toPromise();
    if (updated) {
      this.todos.update(list =>
        list.map(t => (t.id === updated.id ? updated : t))
      );
    }
  }

  async remove(id: number) {
    await this.http.delete(`${this.apiUrl}/${id}`).toPromise();
    this.todos.update(list => list.filter(t => t.id !== id));
  }
}
import { Component, inject, OnInit, signal } from '@angular/core';
import { TodoService } from '../../core/services/todo.service';
import { TodoItem } from '../../shared/models/todo.model';

@Component({
  selector: 'app-todo',
  standalone: true,
  templateUrl: './todo.component.html',
  styleUrl: './todo.component.scss'
})
export class TodoComponent implements OnInit {
  private todoService = inject(TodoService);

  todos = this.todoService.todos;
  newTitle = signal('');

  ngOnInit() {
    this.todoService.load();
  }

  addTodo() {
    const title = this.newTitle().trim();
    if (!title) return;

    this.todoService.add(title);
    this.newTitle.set('');
  }

  toggleTodo(todo: TodoItem) {
    this.todoService.toggle(todo);
  }

  removeTodo(id: number) {
    this.todoService.remove(id);
  }
}
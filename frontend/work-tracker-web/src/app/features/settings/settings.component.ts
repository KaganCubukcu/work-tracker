import { Component, inject, OnInit, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { BreakSlotService } from '../../core/services/break-slot.service';
import { BreakSlot } from '../../shared/models/break-slot.model';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.scss'
})
export class SettingsComponent implements OnInit {
  private breakService = inject(BreakSlotService);

  breaks = this.breakService.breaks;

  newLabel = signal('');
  newStart = signal('');
  newEnd = signal('');

  editingId = signal<number | null>(null);
  editLabel = signal('');
  editStart = signal('');
  editEnd = signal('');

  ngOnInit() {
    this.breakService.load();
  }

  addBreak() {
    const label = this.newLabel().trim();
    const start = this.newStart();
    const end = this.newEnd();

    if (!label || !start || !end) return;

    this.breakService.add(label, `${start}:00`, `${end}:00`);

    this.newLabel.set('');
    this.newStart.set('');
    this.newEnd.set('');
  }

  startEdit(slot: BreakSlot) {
    this.editingId.set(slot.id);
    this.editLabel.set(slot.label);
    this.editStart.set(slot.startTime.slice(0, 5));
    this.editEnd.set(slot.endTime.slice(0, 5));
  }

  cancelEdit() {
    this.editingId.set(null);
  }

  saveEdit(id: number) {
    const label = this.editLabel().trim();
    const start = this.editStart();
    const end = this.editEnd();

    if (!label || !start || !end) return;

    this.breakService.update(id, label, `${start}:00`, `${end}:00`);
    this.editingId.set(null);
  }

  removeBreak(id: number) {
    this.breakService.remove(id);
  }
}
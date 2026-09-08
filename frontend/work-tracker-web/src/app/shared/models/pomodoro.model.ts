export enum PomodoroSessionType {
  Work = 0,
  ShortBreak = 1,
  LongBreak = 2,
}

export interface PomodoroSession {
  id: string;
  type: PomodoroSessionType;
  startedAt: string;
  completedAt: string | null;
}

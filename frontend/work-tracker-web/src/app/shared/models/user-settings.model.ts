export interface UserSettings {
  id: string;
  hireDate: string | null;
  pomodoroWorkMinutes: number;
  pomodoroShortBreakMinutes: number;
  pomodoroLongBreakMinutes: number;
  pomodoroRoundsBeforeLongBreak: number;
}
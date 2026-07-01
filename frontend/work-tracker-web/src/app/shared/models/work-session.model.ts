export interface WorkSession {
  id: number;
  date: string;
  startTime: string;
  expectedDailyHours: number;
  isManuallyEdited: boolean;
}
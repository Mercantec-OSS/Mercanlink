export interface User {
  id: string;
  email: string;
  username: string;
  discordId: string;
  globalName: string;
  avatarUrl: string;
  firstName: string;
  surnameInitial: string;
  passwordChanged: boolean;
  studentId: string;
  department: string;
  employeeType: string;
  adCreatedAt: string;
  lastAdSync: string;
  experience: number;
  level: number;
  roles: string[];
  isActive: boolean;
  createdAt: string;
} 
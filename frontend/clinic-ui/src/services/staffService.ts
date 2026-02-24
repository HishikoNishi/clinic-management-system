import type { Staff } from '@/types/staff'

export const getStaffs = async (): Promise<Staff[]> => {
  return [
    {
      id: 1,
      name: 'Dr. Strange',
      email: 'strange@clinic.com',
      role: 'Doctor',
      isActive: true
    },
    {
      id: 2,
      name: 'Anna Nurse',
      email: 'anna@clinic.com',
      role: 'Nurse',
      isActive: false
    }
  ]
}

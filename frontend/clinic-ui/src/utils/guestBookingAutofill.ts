export type GuestPatientBookingForm = {
  fullName: string
  dateOfBirth: string
  gender: string
  phone: string
  email: string
  address: string
  CitizenId: string
  insuranceNumber: string
}

const lastNames = ['Nguyễn', 'Trần', 'Lê', 'Phạm', 'Hoàng', 'Võ', 'Đặng']
const middleNames = ['Hân', 'Thị', 'Đức', 'Gia', 'Minh', 'Quốc']
const firstNames = ['An', 'Bảo', 'Châu', 'Dung', 'Huy', 'Linh', 'Nam', 'Trang']
const streets = ['Lê Lợi', 'Nguyễn Huệ', 'Trần Hưng Đạo', 'Hai Bà Trưng', 'Võ Thị Sáu']
const districts = ['Quận 1', 'Quận 3', 'Quận 5', 'Quận 7', 'Bình Thạnh', 'Gò Vấp']

const pick = <T,>(items: T[]) => items[Math.floor(Math.random() * items.length)]

const randomDigits = (length: number) =>
  Array.from({ length }, () => Math.floor(Math.random() * 10)).join('')

const randomDateOfBirth = () => {
  const start = new Date(1960, 0, 1).getTime()
  const end = new Date(2018, 11, 31).getTime()
  const value = new Date(start + Math.random() * (end - start))
  return value.toISOString().slice(0, 10)
}

export function fillRandomGuestPatient(form: GuestPatientBookingForm) {
  const fullName = `${pick(lastNames)} ${pick(middleNames)} ${pick(firstNames)}`
  const phone = `0${Math.floor(Math.random() * 9) + 1}${randomDigits(8)}`
  const email = `tranggiabao2007@gmail.com`
  const address = `${Math.floor(Math.random() * 200) + 1} ${pick(streets)}, ${pick(districts)}`
  const citizenId = randomDigits(12)
  const shouldFillInsurance = Math.random() > 0.35
  const insuranceNumber = shouldFillInsurance ? `BHYT${randomDigits(10)}` : ''

  form.fullName = fullName
  form.dateOfBirth = randomDateOfBirth()
  form.gender = Math.random() > 0.5 ? '1' : '2'
  form.phone = phone
  form.email = email
  form.address = address
  form.CitizenId = citizenId
  form.insuranceNumber = insuranceNumber
}

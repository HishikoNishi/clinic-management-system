import { ref } from 'vue'

export type ToastType = 'success' | 'error' | 'warning' | 'info'

export interface ToastMessage {
  id: number
  type: ToastType
  message: string
  duration: number
}

const toasts = ref<ToastMessage[]>([])

const pushToast = (type: ToastType, message: string, duration = 3000) => {
  const id = Date.now() + Math.floor(Math.random() * 1000)
  const item: ToastMessage = { id, type, message, duration }
  toasts.value.push(item)

  if (duration > 0) {
    window.setTimeout(() => {
      toasts.value = toasts.value.filter((t) => t.id !== id)
    }, duration)
  }

  return id
}

const removeToast = (id: number) => {
  toasts.value = toasts.value.filter((t) => t.id !== id)
}

export const useToast = () => ({
  toasts,
  show: (message: string, type: ToastType = 'info', duration = 3000) => pushToast(type, message, duration),
  success: (message: string, duration = 3000) => pushToast('success', message, duration),
  error: (message: string, duration = 3500) => pushToast('error', message, duration),
  warning: (message: string, duration = 3500) => pushToast('warning', message, duration),
  info: (message: string, duration = 3000) => pushToast('info', message, duration),
  remove: removeToast
})

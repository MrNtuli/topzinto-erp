import * as signalR from '@microsoft/signalr'
import type { ChatMessage } from '@/api/chat'

export function createChatConnection(getToken: () => string | null) {
  return new signalR.HubConnectionBuilder()
    .withUrl('/hubs/chat', {
      accessTokenFactory: () => getToken() ?? '',
    })
    .withAutomaticReconnect()
    .build()
}

export type ReceiveMessageHandler = (message: ChatMessage) => void

import { writable } from 'svelte/store';

export const auth = writable<{
  accessToken: string | null;
  refreshToken: string | null;
  user: any | null;
}>({
  accessToken: null,
  refreshToken: null,
  user: null
});
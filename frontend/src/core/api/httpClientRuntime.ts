type RuntimeHandlers = {
  getAccessToken: () => string | null;
  refreshSession: () => Promise<boolean>;
};

let handlers: RuntimeHandlers = {
  getAccessToken: () => null,
  refreshSession: async () => false
};

export function configureHttpClientRuntime(nextHandlers: RuntimeHandlers) {
  handlers = nextHandlers;
}

export function getHttpClientRuntime() {
  return handlers;
}

import { Component, type ErrorInfo, type PropsWithChildren, type ReactNode } from "react";

interface ErrorBoundaryState {
  hasError: boolean;
  error: Error | null;
}

export class ErrorBoundary extends Component<PropsWithChildren, ErrorBoundaryState> {
  constructor(props: PropsWithChildren) {
    super(props);
    this.state = { hasError: false, error: null };
  }

  static getDerivedStateFromError(error: Error): ErrorBoundaryState {
    return { hasError: true, error };
  }

  componentDidCatch(error: Error, info: ErrorInfo) {
    console.error("[ErrorBoundary]", error, info.componentStack);
  }

  render(): ReactNode {
    if (this.state.hasError) {
      return (
        <div className="error-boundary">
          <div className="error-boundary__card">
            <h1 className="error-boundary__title">Something went wrong</h1>
            <p className="error-boundary__description">
              An unexpected error occurred. Please reload the page.
            </p>
            {this.state.error && (
              <pre className="error-boundary__detail">{this.state.error.message}</pre>
            )}
            <button
              type="button"
              className="action-btn action-btn--primary"
              onClick={() => window.location.reload()}
            >
              Reload Page
            </button>
          </div>
        </div>
      );
    }

    return this.props.children;
  }
}

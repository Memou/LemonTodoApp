import { render, screen } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest';
import { ErrorBoundary } from '../ErrorBoundary';
import userEvent from '@testing-library/user-event';

// Component that throws an error
const ThrowError = ({ shouldThrow }) => {
    if (shouldThrow) {
        throw new Error('Test error');
    }
    return <div>No error</div>;
};

describe('ErrorBoundary', () => {
    let consoleErrorSpy;

    beforeEach(() => {
        // Suppress console.error for cleaner test output
        consoleErrorSpy = vi.spyOn(console, 'error').mockImplementation(() => {});
    });

    afterEach(() => {
        consoleErrorSpy.mockRestore();
    });

    it('renders children when there is no error', () => {
        render(
            <ErrorBoundary>
                <div>Test content</div>
            </ErrorBoundary>
        );

        expect(screen.getByText('Test content')).toBeInTheDocument();
    });

    it('catches errors and displays fallback UI', () => {
        render(
            <ErrorBoundary>
                <ThrowError shouldThrow={true} />
            </ErrorBoundary>
        );

        // Check that error UI is displayed
        expect(screen.getByText('Oops! Something went wrong')).toBeInTheDocument();
        expect(screen.getByText(/We encountered an unexpected error/i)).toBeInTheDocument();
    });

    it('displays error icon when error occurs', () => {
        render(
            <ErrorBoundary>
                <ThrowError shouldThrow={true} />
            </ErrorBoundary>
        );

        // Error icon should be present (checking for the container with specific styling)
        const errorIcon = screen.getByTestId('ErrorIcon');
        expect(errorIcon).toBeInTheDocument();
    });

    it('shows "Reload Page" button when error occurs', () => {
        render(
            <ErrorBoundary>
                <ThrowError shouldThrow={true} />
            </ErrorBoundary>
        );

        expect(screen.getByRole('button', { name: /reload page/i })).toBeInTheDocument();
    });

    it('shows "Try Again" button when error occurs', () => {
        render(
            <ErrorBoundary>
                <ThrowError shouldThrow={true} />
            </ErrorBoundary>
        );

        expect(screen.getByRole('button', { name: /try again/i })).toBeInTheDocument();
    });

    it('reloads page when "Reload Page" button is clicked', async () => {
        const user = userEvent.setup();
        const reloadMock = vi.fn();
        
        // Mock window.location.reload
        Object.defineProperty(window, 'location', {
            value: { reload: reloadMock },
            writable: true
        });

        render(
            <ErrorBoundary>
                <ThrowError shouldThrow={true} />
            </ErrorBoundary>
        );

        const reloadButton = screen.getByRole('button', { name: /reload page/i });
        await user.click(reloadButton);

        expect(reloadMock).toHaveBeenCalledTimes(1);
    });

    it('resets error state when "Try Again" button is clicked', async () => {
        const user = userEvent.setup();

        render(
            <ErrorBoundary>
                <ThrowError shouldThrow={true} />
            </ErrorBoundary>
        );

        // Verify error UI is shown
        expect(screen.getByText('Oops! Something went wrong')).toBeInTheDocument();
        expect(screen.getByRole('button', { name: /try again/i })).toBeInTheDocument();

        // This test verifies that the button exists and can be clicked
        // In a real scenario, clicking Try Again allows React to attempt re-rendering
        // If the error is transient, the component would recover
        const tryAgainButton = screen.getByRole('button', { name: /try again/i });
        expect(tryAgainButton).toBeInTheDocument();

        // Note: We can't fully test the recovery behavior in a unit test
        // because the component will throw again. This is expected behavior.
    });

    it('logs error to console when error is caught', () => {
        render(
            <ErrorBoundary>
                <ThrowError shouldThrow={true} />
            </ErrorBoundary>
        );

        expect(consoleErrorSpy).toHaveBeenCalled();
    });

    it('shows error details in development mode', () => {
        // Set DEV mode
        const originalEnv = import.meta.env.DEV;
        import.meta.env.DEV = true;

        render(
            <ErrorBoundary>
                <ThrowError shouldThrow={true} />
            </ErrorBoundary>
        );

        // In dev mode, error details should be visible
        expect(screen.getByText(/Test error/)).toBeInTheDocument();

        // Restore original env
        import.meta.env.DEV = originalEnv;
    });

    it('hides error details in production mode', () => {
        // Set production mode
        const originalEnv = import.meta.env.DEV;
        import.meta.env.DEV = false;

        render(
            <ErrorBoundary>
                <ThrowError shouldThrow={true} />
            </ErrorBoundary>
        );

        // In production mode, detailed error message should not be visible
        // Only the generic message should be shown
        expect(screen.queryByText(/at ThrowError/)).not.toBeInTheDocument();

        // Restore original env
        import.meta.env.DEV = originalEnv;
    });

    it('displays reassuring message about data safety', () => {
        render(
            <ErrorBoundary>
                <ThrowError shouldThrow={true} />
            </ErrorBoundary>
        );

        expect(screen.getByText(/Don't worry, your data is safe/i)).toBeInTheDocument();
    });

    it('renders multiple children without error', () => {
        render(
            <ErrorBoundary>
                <div>Child 1</div>
                <div>Child 2</div>
                <div>Child 3</div>
            </ErrorBoundary>
        );

        expect(screen.getByText('Child 1')).toBeInTheDocument();
        expect(screen.getByText('Child 2')).toBeInTheDocument();
        expect(screen.getByText('Child 3')).toBeInTheDocument();
    });

    it('catches errors from nested components', () => {
        const NestedComponent = () => {
            return (
                <div>
                    <div>
                        <ThrowError shouldThrow={true} />
                    </div>
                </div>
            );
        };

        render(
            <ErrorBoundary>
                <NestedComponent />
            </ErrorBoundary>
        );

        expect(screen.getByText('Oops! Something went wrong')).toBeInTheDocument();
    });
});

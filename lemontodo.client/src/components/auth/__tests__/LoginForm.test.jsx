import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';
import { ThemeProvider } from '@mui/material';
import { createTheme } from '@mui/material/styles';
import { LoginForm } from '../LoginForm';

const theme = createTheme();

describe('LoginForm', () => {
    const defaultProps = {
        onLogin: vi.fn(),
        onRegister: vi.fn(),
        error: null,
    };

    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('renders taskflow branding', () => {
        render(
            <ThemeProvider theme={theme}>
                <LoginForm {...defaultProps} />
            </ThemeProvider>
        );

        expect(screen.getByText('taskflow')).toBeInTheDocument();
    });

    it('renders sign in and sign up tabs', () => {
        render(
            <ThemeProvider theme={theme}>
                <LoginForm {...defaultProps} />
            </ThemeProvider>
        );

        // Tabs are rendered as buttons with role="tab"
        const tabs = screen.getAllByRole('tab');
        expect(tabs).toHaveLength(2);
        expect(tabs[0]).toHaveTextContent('Sign In');
        expect(tabs[1]).toHaveTextContent('Sign Up');
    });

    it('renders username and password fields', () => {
        render(
            <ThemeProvider theme={theme}>
                <LoginForm {...defaultProps} />
            </ThemeProvider>
        );

        expect(screen.getByLabelText(/username/i)).toBeInTheDocument();
        expect(screen.getByLabelText(/password/i)).toBeInTheDocument();
    });

    it('calls onLogin when sign in form is submitted', async () => {
        defaultProps.onLogin.mockResolvedValue({ success: true });

        render(
            <ThemeProvider theme={theme}>
                <LoginForm {...defaultProps} />
            </ThemeProvider>
        );

        const usernameInput = screen.getByLabelText(/username/i);
        const passwordInput = screen.getByLabelText(/password/i);
        const submitButton = screen.getByRole('button', { name: /sign in/i });

        fireEvent.change(usernameInput, { target: { value: 'testuser' } });
        fireEvent.change(passwordInput, { target: { value: 'password123' } });
        fireEvent.click(submitButton);

        await waitFor(() => {
            expect(defaultProps.onLogin).toHaveBeenCalledWith('testuser', 'password123');
        });
    });

    it('calls onRegister when sign up form is submitted', async () => {
        defaultProps.onRegister.mockResolvedValue({ success: true });

        render(
            <ThemeProvider theme={theme}>
                <LoginForm {...defaultProps} />
            </ThemeProvider>
        );

        // Switch to sign up tab
        const signUpTab = screen.getByText('Sign Up');
        fireEvent.click(signUpTab);

        const usernameInput = screen.getByLabelText(/username/i);
        const passwordInput = screen.getByLabelText(/password/i);
        const submitButton = screen.getByRole('button', { name: /create account/i });

        fireEvent.change(usernameInput, { target: { value: 'newuser' } });
        fireEvent.change(passwordInput, { target: { value: 'newpass123' } });
        fireEvent.click(submitButton);

        await waitFor(() => {
            expect(defaultProps.onRegister).toHaveBeenCalledWith('newuser', 'newpass123');
        });
    });

    it('displays error message when provided', () => {
        render(
            <ThemeProvider theme={theme}>
                <LoginForm {...defaultProps} error="Invalid credentials" />
            </ThemeProvider>
        );

        expect(screen.getByText('Invalid credentials')).toBeInTheDocument();
    });

    it('disables button and shows loading spinner during submission', async () => {
        defaultProps.onLogin.mockImplementation(() => new Promise(resolve => setTimeout(() => resolve({ success: true }), 100)));

        render(
            <ThemeProvider theme={theme}>
                <LoginForm {...defaultProps} />
            </ThemeProvider>
        );

        const usernameInput = screen.getByLabelText(/username/i);
        const passwordInput = screen.getByLabelText(/password/i);
        const submitButton = screen.getByRole('button', { name: /sign in/i });

        fireEvent.change(usernameInput, { target: { value: 'testuser' } });
        fireEvent.change(passwordInput, { target: { value: 'password123' } });
        fireEvent.click(submitButton);

        // Button should be disabled during loading
        expect(submitButton).toBeDisabled();
        expect(screen.getByRole('progressbar')).toBeInTheDocument();

        await waitFor(() => {
            expect(submitButton).not.toBeDisabled();
        });
    });

    it('clears form after successful submission', async () => {
        defaultProps.onLogin.mockResolvedValue({ success: true });

        render(
            <ThemeProvider theme={theme}>
                <LoginForm {...defaultProps} />
            </ThemeProvider>
        );

        const usernameInput = screen.getByLabelText(/username/i);
        const passwordInput = screen.getByLabelText(/password/i);

        fireEvent.change(usernameInput, { target: { value: 'testuser' } });
        fireEvent.change(passwordInput, { target: { value: 'password123' } });
        fireEvent.submit(screen.getByRole('button', { name: /sign in/i }).closest('form'));

        await waitFor(() => {
            expect(usernameInput.value).toBe('');
            expect(passwordInput.value).toBe('');
        });
    });
});

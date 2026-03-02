import { render, screen, fireEvent } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';
import { ThemeProvider } from '@mui/material';
import { createTheme } from '@mui/material/styles';
import { AppHeader } from '../AppHeader';

const theme = createTheme();

describe('AppHeader', () => {
    const defaultProps = {
        username: 'testuser',
        onLogout: vi.fn(),
        onExport: vi.fn(),
        onImport: vi.fn(),
        tasksCount: 5,
    };

    it('renders taskflow branding', () => {
        render(
            <ThemeProvider theme={theme}>
                <AppHeader {...defaultProps} />
            </ThemeProvider>
        );

        expect(screen.getByText('taskflow')).toBeInTheDocument();
    });

    it('displays username', () => {
        render(
            <ThemeProvider theme={theme}>
                <AppHeader {...defaultProps} />
            </ThemeProvider>
        );

        expect(screen.getByText('testuser')).toBeInTheDocument();
    });

    it('calls onLogout when sign out button is clicked', () => {
        render(
            <ThemeProvider theme={theme}>
                <AppHeader {...defaultProps} />
            </ThemeProvider>
        );

        const signOutButton = screen.getByText('Sign Out');
        fireEvent.click(signOutButton);

        expect(defaultProps.onLogout).toHaveBeenCalledTimes(1);
    });

    it('disables export button when no tasks', () => {
        render(
            <ThemeProvider theme={theme}>
                <AppHeader {...defaultProps} tasksCount={0} />
            </ThemeProvider>
        );

        const exportButton = screen.getByText('Export');
        expect(exportButton).toBeDisabled();
    });

    it('enables export button when tasks exist', () => {
        render(
            <ThemeProvider theme={theme}>
                <AppHeader {...defaultProps} tasksCount={5} />
            </ThemeProvider>
        );

        const exportButton = screen.getByText('Export');
        expect(exportButton).not.toBeDisabled();
    });

    it('shows export menu when export button is clicked', () => {
        render(
            <ThemeProvider theme={theme}>
                <AppHeader {...defaultProps} />
            </ThemeProvider>
        );

        const exportButton = screen.getByText('Export');
        fireEvent.click(exportButton);

        expect(screen.getByText('Export as JSON')).toBeInTheDocument();
        expect(screen.getByText('Export as CSV')).toBeInTheDocument();
    });

    it('calls onExport with correct format', () => {
        render(
            <ThemeProvider theme={theme}>
                <AppHeader {...defaultProps} />
            </ThemeProvider>
        );

        const exportButton = screen.getByText('Export');
        fireEvent.click(exportButton);

        const jsonOption = screen.getByText('Export as JSON');
        fireEvent.click(jsonOption);

        expect(defaultProps.onExport).toHaveBeenCalledWith('json');
    });

    it('renders import button', () => {
        render(
            <ThemeProvider theme={theme}>
                <AppHeader {...defaultProps} />
            </ThemeProvider>
        );

        expect(screen.getByText('Import')).toBeInTheDocument();
    });
});

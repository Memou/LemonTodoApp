import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { ThemeProvider } from '@mui/material';
import { createTheme } from '@mui/material/styles';
import { CreateTaskModal } from '../CreateTaskModal';

const theme = createTheme();

describe('CreateTaskModal', () => {
    const defaultProps = {
        open: true,
        onClose: vi.fn(),
        onSubmit: vi.fn(),
        error: null,
    };

    beforeEach(() => {
        vi.clearAllMocks();
    });

    it('renders modal when open', () => {
        render(
            <ThemeProvider theme={theme}>
                <CreateTaskModal {...defaultProps} />
            </ThemeProvider>
        );

        expect(screen.getByText('Create New Task')).toBeInTheDocument();
    });

    it('does not render when closed', () => {
        render(
            <ThemeProvider theme={theme}>
                <CreateTaskModal {...defaultProps} open={false} />
            </ThemeProvider>
        );

        expect(screen.queryByText('Create New Task')).not.toBeInTheDocument();
    });

    it('renders all form fields', () => {
        render(
            <ThemeProvider theme={theme}>
                <CreateTaskModal {...defaultProps} />
            </ThemeProvider>
        );

        expect(screen.getByLabelText(/task title/i)).toBeInTheDocument();
        expect(screen.getByLabelText(/description/i)).toBeInTheDocument();
        // MUI date fields - query by type
        expect(document.querySelector('input[type="date"]')).toBeInTheDocument();
        // Priority select shows the default value
        expect(screen.getByText('Medium Priority')).toBeInTheDocument();
    });

    it('renders priority options', async () => {
        render(
            <ThemeProvider theme={theme}>
                <CreateTaskModal {...defaultProps} />
            </ThemeProvider>
        );

        // MUI Select renders as a div with role="combobox"; open with mouseDown
        const selectDiv = document.querySelector('[role="combobox"]');
        fireEvent.mouseDown(selectDiv);

        await waitFor(() => {
            expect(screen.getByRole('option', { name: 'Low Priority' })).toBeInTheDocument();
            expect(screen.getByRole('option', { name: 'Medium Priority' })).toBeInTheDocument();
            expect(screen.getByRole('option', { name: 'High Priority' })).toBeInTheDocument();
            expect(screen.getByRole('option', { name: 'Urgent' })).toBeInTheDocument();
        });
    });

    it('calls onSubmit with correct data when form is submitted', async () => {
        render(
            <ThemeProvider theme={theme}>
                <CreateTaskModal {...defaultProps} />
            </ThemeProvider>
        );

        const titleInput = screen.getByLabelText(/task title/i);
        const descriptionInput = screen.getByLabelText(/description/i);

        fireEvent.change(titleInput, { target: { value: 'New Task' } });
        fireEvent.change(descriptionInput, { target: { value: 'Task description' } });

        const submitButton = screen.getByText('Create Task');
        fireEvent.click(submitButton);

        await waitFor(() => {
            expect(defaultProps.onSubmit).toHaveBeenCalledWith(
                expect.objectContaining({
                    title: 'New Task',
                    description: 'Task description',
                    priority: 1, // Default medium priority
                })
            );
        });
    });

    it('calls onClose when Cancel button is clicked', () => {
        render(
            <ThemeProvider theme={theme}>
                <CreateTaskModal {...defaultProps} />
            </ThemeProvider>
        );

        const cancelButton = screen.getByText('Cancel');
        fireEvent.click(cancelButton);

        expect(defaultProps.onClose).toHaveBeenCalledTimes(1);
    });

    it('displays error message when provided', () => {
        render(
            <ThemeProvider theme={theme}>
                <CreateTaskModal {...defaultProps} error="Validation failed" />
            </ThemeProvider>
        );

        expect(screen.getByText('Validation failed')).toBeInTheDocument();
    });

    it('requires title field', async () => {
        render(
            <ThemeProvider theme={theme}>
                <CreateTaskModal {...defaultProps} />
            </ThemeProvider>
        );

        const titleInput = screen.getByLabelText(/task title/i);
        expect(titleInput).toBeRequired();
    });

    it('allows changing priority', async () => {
        render(
            <ThemeProvider theme={theme}>
                <CreateTaskModal {...defaultProps} />
            </ThemeProvider>
        );

        // Open the MUI Select dropdown
        const selectDiv = document.querySelector('[role="combobox"]');
        fireEvent.mouseDown(selectDiv);

        // Wait for options to appear and click High Priority
        const highPriority = await screen.findByRole('option', { name: 'High Priority' });
        fireEvent.click(highPriority);

        // After selection the combobox should show the selected value
        await waitFor(() => {
            expect(selectDiv).toHaveTextContent('High Priority');
        });
    });

    it('resets form when closed', () => {
        const { rerender } = render(
            <ThemeProvider theme={theme}>
                <CreateTaskModal {...defaultProps} open={true} />
            </ThemeProvider>
        );

        const titleInput = screen.getByLabelText(/task title/i);
        fireEvent.change(titleInput, { target: { value: 'Test Task' } });

        const cancelButton = screen.getByText('Cancel');
        fireEvent.click(cancelButton);

        rerender(
            <ThemeProvider theme={theme}>
                <CreateTaskModal {...defaultProps} open={true} />
            </ThemeProvider>
        );

        expect(screen.getByLabelText(/task title/i)).toHaveValue('');
    });
});

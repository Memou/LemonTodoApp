import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { describe, it, expect, vi, beforeEach } from 'vitest';
import { ThemeProvider } from '@mui/material';
import { createTheme } from '@mui/material/styles';
import { EditTaskModal } from '../EditTaskModal';

const theme = createTheme();

describe('EditTaskModal', () => {
    const mockTask = {
        id: '1',
        title: 'Existing Task',
        description: 'Existing description',
        priority: 2,
        dueDate: '2024-12-31T00:00:00Z',
        isCompleted: false,
    };

    const defaultProps = {
        open: true,
        task: mockTask,
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
                <EditTaskModal {...defaultProps} />
            </ThemeProvider>
        );

        expect(screen.getByText('Edit Task')).toBeInTheDocument();
    });

    it('does not render when closed', () => {
        render(
            <ThemeProvider theme={theme}>
                <EditTaskModal {...defaultProps} open={false} />
            </ThemeProvider>
        );

        expect(screen.queryByText('Edit Task')).not.toBeInTheDocument();
    });

    it('pre-fills form with task data', () => {
        render(
            <ThemeProvider theme={theme}>
                <EditTaskModal {...defaultProps} />
            </ThemeProvider>
        );

        const titleInput = screen.getByLabelText(/task title/i);
        const descriptionInput = screen.getByLabelText(/description/i);

        expect(titleInput).toHaveValue('Existing Task');
        expect(descriptionInput).toHaveValue('Existing description');
    });

    it('pre-selects correct priority', () => {
        render(
            <ThemeProvider theme={theme}>
                <EditTaskModal {...defaultProps} />
            </ThemeProvider>
        );

        // Check that High Priority is displayed (priority 2 = High)
        expect(screen.getByText('High Priority')).toBeInTheDocument();
    });

    it('calls onSubmit with updated data', async () => {
        render(
            <ThemeProvider theme={theme}>
                <EditTaskModal {...defaultProps} />
            </ThemeProvider>
        );

        const titleInput = screen.getByLabelText(/task title/i);
        fireEvent.change(titleInput, { target: { value: 'Updated Task' } });

        // MUI Button type="submit" click doesn't always trigger form submit in jsdom;
        // submit the form directly instead
        const form = titleInput.closest('form');
        fireEvent.submit(form);

        await waitFor(() => {
            expect(defaultProps.onSubmit).toHaveBeenCalledTimes(1);
            expect(defaultProps.onSubmit).toHaveBeenCalledWith(
                expect.objectContaining({
                    title: 'Updated Task',
                })
            );
        });
    });

    it('calls onClose when Cancel button is clicked', () => {
        render(
            <ThemeProvider theme={theme}>
                <EditTaskModal {...defaultProps} />
            </ThemeProvider>
        );

        const cancelButton = screen.getByText('Cancel');
        fireEvent.click(cancelButton);

        expect(defaultProps.onClose).toHaveBeenCalledTimes(1);
    });

    it('displays error message when provided', () => {
        render(
            <ThemeProvider theme={theme}>
                <EditTaskModal {...defaultProps} error="Update failed" />
            </ThemeProvider>
        );

        expect(screen.getByText('Update failed')).toBeInTheDocument();
    });

    it('handles task with no description', () => {
        const taskWithoutDescription = { ...mockTask, description: null };

        render(
            <ThemeProvider theme={theme}>
                <EditTaskModal {...defaultProps} task={taskWithoutDescription} />
            </ThemeProvider>
        );

        const descriptionInput = screen.getByLabelText(/description/i);
        expect(descriptionInput).toHaveValue('');
    });

    it('handles task with no due date', () => {
        const taskWithoutDueDate = { ...mockTask, dueDate: null };

        render(
            <ThemeProvider theme={theme}>
                <EditTaskModal {...defaultProps} task={taskWithoutDueDate} />
            </ThemeProvider>
        );

        const dueDateInput = screen.getByLabelText(/due date/i);
        // Should have today's date as default
        expect(dueDateInput.value).toBeTruthy();
    });

    it('updates form when task prop changes', () => {
        const { rerender } = render(
            <ThemeProvider theme={theme}>
                <EditTaskModal {...defaultProps} />
            </ThemeProvider>
        );

        const newTask = { ...mockTask, title: 'Different Task', priority: 3 };

        rerender(
            <ThemeProvider theme={theme}>
                <EditTaskModal {...defaultProps} task={newTask} />
            </ThemeProvider>
        );

        const titleInput = screen.getByLabelText(/task title/i);
        expect(titleInput).toHaveValue('Different Task');
    });

    it('allows changing all fields', () => {
        render(
            <ThemeProvider theme={theme}>
                <EditTaskModal {...defaultProps} />
            </ThemeProvider>
        );

        const titleInput = screen.getByLabelText(/task title/i);
        const descriptionInput = screen.getByLabelText(/description/i);

        fireEvent.change(titleInput, { target: { value: 'New Title' } });
        fireEvent.change(descriptionInput, { target: { value: 'New Description' } });

        expect(titleInput).toHaveValue('New Title');
        expect(descriptionInput).toHaveValue('New Description');
    });
});

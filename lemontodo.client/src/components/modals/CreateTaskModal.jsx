import { useState, useEffect, useRef } from 'react';
import {
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    TextField,
    Button,
    Alert,
    FormControl,
    InputLabel,
    Select,
    MenuItem,
} from '@mui/material';
import { getTodayDate } from '../../utils/helpers';

export function CreateTaskModal({ open, onClose, onSubmit, error }) {
    const [taskForm, setTaskForm] = useState({
        title: '',
        description: '',
        priority: 1,
        dueDate: getTodayDate()
    });

    const titleInputRef = useRef(null);
    const dateInputRef = useRef(null);

    // Reset form and auto-focus title field when modal opens
    useEffect(() => {
        if (open) {
            // Reset form to initial state
            setTaskForm({
                title: '',
                description: '',
                priority: 1,
                dueDate: getTodayDate()
            });
            // Delay to ensure dialog is fully rendered
            const timer = setTimeout(() => {
                titleInputRef.current?.focus();
            }, 100);
            return () => clearTimeout(timer);
        }
    }, [open]);

    const handleSubmit = (e) => {
        e.preventDefault();
        onSubmit(taskForm);
    };

    const handleClose = () => {
        setTaskForm({ title: '', description: '', priority: 1, dueDate: getTodayDate() });
        onClose();
    };

    // Auto-open date picker when date field is focused
    const handleDateFieldFocus = () => {
        // Trigger the native date picker to open
        dateInputRef.current?.showPicker?.();
    };

    return (
        <Dialog open={open} onClose={handleClose} maxWidth="sm" fullWidth PaperProps={{ sx: { borderRadius: 4 } }}>
            <DialogTitle sx={{ pb: 2, pt: 3, px: 3, fontSize: '1.5rem', fontWeight: 700 }}>
                Create New Task
            </DialogTitle>
            <form onSubmit={handleSubmit}>
                <DialogContent sx={{ pt: 0 }}>
                    {error && <Alert severity="error" sx={{ mb: 3 }}>{error}</Alert>}
                    <TextField 
                        fullWidth 
                        label="Task Title" 
                        value={taskForm.title} 
                        onChange={(e) => setTaskForm({ ...taskForm, title: e.target.value })} 
                        required 
                        inputRef={titleInputRef}
                        sx={{ mb: 3, mt: 2 }} 
                    />
                    <TextField 
                        fullWidth 
                        label="Description (optional)" 
                        value={taskForm.description} 
                        onChange={(e) => setTaskForm({ ...taskForm, description: e.target.value })} 
                        multiline 
                        rows={3} 
                        sx={{ mb: 3 }} 
                    />
                    <FormControl fullWidth sx={{ mb: 3 }}>
                        <InputLabel>Priority</InputLabel>
                        <Select 
                            value={taskForm.priority} 
                            label="Priority" 
                            onChange={(e) => setTaskForm({ ...taskForm, priority: e.target.value })}
                        >
                            <MenuItem value={0}>Low Priority</MenuItem>
                            <MenuItem value={1}>Medium Priority</MenuItem>
                            <MenuItem value={2}>High Priority</MenuItem>
                            <MenuItem value={3}>Urgent</MenuItem>
                        </Select>
                    </FormControl>
                    <TextField 
                        fullWidth 
                        label="Due Date" 
                        type="date" 
                        value={taskForm.dueDate} 
                        onChange={(e) => setTaskForm({ ...taskForm, dueDate: e.target.value })} 
                        onFocus={handleDateFieldFocus}
                        InputLabelProps={{ shrink: true }} 
                        inputProps={{ min: getTodayDate(), ref: dateInputRef }}
                    />
                </DialogContent>
                <DialogActions sx={{ p: 3, pt: 2 }}>
                    <Button onClick={handleClose} sx={{ color: 'text.secondary' }}>Cancel</Button>
                    <Button type="submit" variant="contained" sx={{ minWidth: 120 }}>Create Task</Button>
                </DialogActions>
            </form>
        </Dialog>
    );
}

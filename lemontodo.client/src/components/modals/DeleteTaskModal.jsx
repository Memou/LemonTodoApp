import {
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    Typography,
    Button,
} from '@mui/material';

export function DeleteTaskModal({ open, taskTitle, onClose, onConfirm }) {
    return (
        <Dialog open={open} onClose={onClose} PaperProps={{ sx: { borderRadius: 4 } }}>
            <DialogTitle sx={{ fontSize: '1.5rem', fontWeight: 700 }}>
                Delete Task
            </DialogTitle>
            <DialogContent>
                <Typography sx={{ mb: 2 }}>
                    Are you sure you want to delete this task?
                </Typography>
                <Typography variant="h6" sx={{ fontWeight: 600 }}>
                    "{taskTitle}"
                </Typography>
            </DialogContent>
            <DialogActions sx={{ p: 3, pt: 2 }}>
                <Button onClick={onClose} sx={{ color: 'text.secondary' }}>
                    Cancel
                </Button>
                <Button onClick={onConfirm} color="error" variant="contained">
                    Delete
                </Button>
            </DialogActions>
        </Dialog>
    );
}

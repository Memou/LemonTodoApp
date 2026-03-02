import { Box, Typography, Button } from '@mui/material';
import { Add as AddIcon } from '@mui/icons-material';

export function EmptyState({ onCreateTask }) {
    return (
        <Box sx={{ textAlign: 'center', py: 10 }}>
            <Typography variant="h5" color="text.primary" gutterBottom fontWeight={600}>
                No tasks yet
            </Typography>
            <Typography variant="body1" color="text.secondary" sx={{ mb: 4 }}>
                Create your first task to get started
            </Typography>
            <Button 
                variant="contained" 
                size="large" 
                startIcon={<AddIcon />} 
                onClick={onCreateTask} 
                sx={{ borderRadius: 32 }}
            >
                Create Your First Task
            </Button>
        </Box>
    );
}

import { Box, Typography, Chip, Stack, Button } from '@mui/material';
import {
    CheckCircle as CheckCircleIcon,
    CalendarToday as CalendarIcon,
} from '@mui/icons-material';
import { getPriorityLabel, getPriorityColor } from '../../utils/helpers';

export function TaskItem({ task, onEdit, onToggleComplete, onDelete }) {
    return (
        <Box 
            onClick={() => onEdit(task)}
            sx={{ 
                py: 1.5, 
                px: 2, 
                mx: -2,
                opacity: task.isCompleted ? 0.6 : 1, 
                bgcolor: 'transparent',
                display: 'flex',
                gap: 2,
                cursor: 'pointer',
                borderRadius: 2,
                transition: 'all 0.2s ease',
                '&:hover': {
                    bgcolor: 'action.hover',
                    transform: 'translateX(4px)'
                }
            }}
        >
            <Box sx={{ flex: 1, minWidth: 0 }}>
                <Stack direction="row" spacing={1} alignItems="center" flexWrap="wrap" sx={{ mb: 0.5 }}>
                    <Typography 
                        variant="h6" 
                        sx={{ 
                            textDecoration: task.isCompleted ? 'line-through' : 'none', 
                            fontSize: '1rem', 
                            fontWeight: 600 
                        }}
                    >
                        {task.title}
                    </Typography>
                    <Chip 
                        label={getPriorityLabel(task.priority)} 
                        size="small" 
                        sx={{ 
                            bgcolor: getPriorityColor(task.priority), 
                            color: 'white', 
                            fontWeight: 600, 
                            height: 20, 
                            fontSize: '0.7rem' 
                        }} 
                    />
                    {task.completedAt && (
                        <Chip 
                            icon={<CheckCircleIcon sx={{ fontSize: 14 }} />} 
                            label={`Completed ${new Date(task.completedAt).toLocaleDateString()}`} 
                            size="small" 
                            color="success" 
                            variant="outlined" 
                            sx={{ height: 20, fontSize: '0.7rem' }} 
                        />
                    )}
                </Stack>

                {task.description && (
                    <Typography 
                        variant="body2" 
                        color="text.secondary" 
                        sx={{ mb: 0.5, lineHeight: 1.4, fontSize: '0.875rem' }}
                    >
                        {task.description}
                    </Typography>
                )}

                <Stack direction="row" spacing={1} flexWrap="wrap">
                    {task.dueDate && (
                        <Chip 
                            icon={<CalendarIcon sx={{ fontSize: 14 }} />} 
                            label={`Due ${new Date(task.dueDate).toLocaleDateString()}`} 
                            size="small" 
                            variant="outlined" 
                            sx={{ 
                                borderColor: 'text.secondary', 
                                color: 'text.secondary', 
                                height: 20, 
                                fontSize: '0.7rem' 
                            }} 
                        />
                    )}
                </Stack>
            </Box>

            <Stack 
                direction="row" 
                spacing={0.5} 
                sx={{ flexShrink: 0, alignSelf: 'center' }} 
                onClick={(e) => e.stopPropagation()}
            >
                {task.isCompleted ? (
                    <Button 
                        size="small" 
                        variant="outlined"
                        onClick={(e) => { e.stopPropagation(); onToggleComplete(task); }}
                        sx={{ 
                            borderRadius: 16, 
                            textTransform: 'none', 
                            borderColor: 'text.secondary', 
                            color: 'text.secondary', 
                            minWidth: 'auto', 
                            px: 2.4, 
                            py: 0.6, 
                            fontSize: '0.96rem' 
                        }}
                    >
                        Undo
                    </Button>
                ) : (
                    <Button 
                        size="small" 
                        variant="contained"
                        onClick={(e) => { e.stopPropagation(); onToggleComplete(task); }}
                        sx={{ 
                            borderRadius: 16, 
                            textTransform: 'none',
                            bgcolor: '#10b981',
                            color: 'white',
                            minWidth: 'auto',
                            px: 2.4,
                            py: 0.6,
                            fontSize: '0.96rem',
                            '&:hover': {
                                bgcolor: '#34d399'
                            }
                        }}
                    >
                        Complete
                    </Button>
                )}
                <Button 
                    size="small" 
                    variant="outlined"
                    onClick={(e) => { e.stopPropagation(); onEdit(task); }}
                    sx={{ 
                        borderRadius: 16, 
                        textTransform: 'none', 
                        minWidth: 'auto', 
                        px: 2.4, 
                        py: 0.6, 
                        fontSize: '0.96rem' 
                    }}
                >
                    Edit
                </Button>
                <Button 
                    size="small" 
                    variant="outlined"
                    color="error"
                    onClick={(e) => { e.stopPropagation(); onDelete(task); }}
                    sx={{ 
                        borderRadius: 16, 
                        textTransform: 'none', 
                        minWidth: 'auto', 
                        px: 2.4, 
                        py: 0.6, 
                        fontSize: '0.96rem' 
                    }}
                >
                    Delete
                </Button>
            </Stack>
        </Box>
    );
}

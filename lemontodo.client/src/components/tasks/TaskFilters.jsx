import {
    Stack,
    Typography,
    Button,
    FormControl,
    InputLabel,
    Select,
    MenuItem,
    IconButton,
} from '@mui/material';
import {
    Add as AddIcon,
    ArrowUpward as ArrowUpwardIcon,
    ArrowDownward as ArrowDownwardIcon,
} from '@mui/icons-material';

export function TaskFilters({ 
    filter, 
    setFilter, 
    sortBy, 
    setSortBy, 
    sortDirection, 
    setSortDirection, 
    onCreateTask,
    hasActiveTasks 
}) {
    return (
        <Stack direction="row" spacing={2} alignItems="center" flexWrap="wrap" sx={{ mb: 3 }}>
            <Typography variant="h5" sx={{ flexGrow: 1, minWidth: '150px', fontWeight: 700 }}>
                Your Tasks
            </Typography>
            {hasActiveTasks && (
                <>
                    <Button 
                        variant="contained" 
                        startIcon={<AddIcon />} 
                        onClick={onCreateTask}
                        sx={{ borderRadius: 32, display: { xs: 'none', sm: 'flex' } }}
                    >
                        New Task
                    </Button>
                    <Button 
                        variant="contained" 
                        fullWidth
                        startIcon={<AddIcon />} 
                        onClick={onCreateTask}
                        sx={{ borderRadius: 32, display: { xs: 'flex', sm: 'none' }, width: '100%', order: 10 }}
                    >
                        New Task
                    </Button>
                </>
            )}
            <FormControl size="small" sx={{ minWidth: 130 }}>
                <InputLabel>Filter</InputLabel>
                <Select value={filter} label="Filter" onChange={(e) => setFilter(e.target.value)}>
                    <MenuItem value="all">All Tasks</MenuItem>
                    <MenuItem value="pending">Pending</MenuItem>
                    <MenuItem value="completed">Completed</MenuItem>
                </Select>
            </FormControl>
            <FormControl size="small" sx={{ minWidth: 150 }}>
                <InputLabel>Sort by</InputLabel>
                <Select value={sortBy} label="Sort by" onChange={(e) => setSortBy(e.target.value)}>
                    <MenuItem value="createdAt">Date Created</MenuItem>
                    <MenuItem value="priority">Priority</MenuItem>
                    <MenuItem value="dueDate">Due Date</MenuItem>
                    <MenuItem value="title">Title</MenuItem>
                </Select>
            </FormControl>
            <IconButton 
                size="small" 
                onClick={() => setSortDirection(sortDirection === 'desc' ? 'asc' : 'desc')} 
                sx={{ bgcolor: 'action.hover' }}
            >
                {sortDirection === 'desc' ? <ArrowDownwardIcon fontSize="small" /> : <ArrowUpwardIcon fontSize="small" />}
            </IconButton>
        </Stack>
    );
}

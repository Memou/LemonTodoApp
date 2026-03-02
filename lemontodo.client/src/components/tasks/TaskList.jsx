import { List, Box, Divider } from '@mui/material';
import { TaskItem } from './TaskItem';

export function TaskList({ tasks, onEdit, onToggleComplete, onDelete }) {
    return (
        <List sx={{ width: '100%' }}>
            {tasks.map((task, index) => (
                <Box key={task.id}>
                    {index > 0 && <Divider sx={{ my: 1.5 }} />}
                    <TaskItem
                        task={task}
                        onEdit={onEdit}
                        onToggleComplete={onToggleComplete}
                        onDelete={onDelete}
                    />
                </Box>
            ))}
        </List>
    );
}

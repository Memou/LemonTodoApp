# 401 Unauthorized Error Fix & Date Picker Improvements

## Issues Fixed

### 1. ✅ Added Debug Logging for 401 Error

The 401 error is likely caused by one of these issues:

#### **Most Common Cause: Server Restart**
If you restart the server, the JWT secret regenerates, invalidating all existing tokens.

**Solution**: Logout and login again after server restart.

#### **Automatic Fix Added**:
```javascript
if (response.status === 401) {
    setError('Session expired. Please login again.');
    handleLogout(); // Auto-logout on 401
    return;
}
```

Now when you get a 401:
- You'll see "Session expired. Please login again."
- You'll be automatically logged out
- You can login again to get a new token

### 2. ✅ Enhanced Server-Side Logging

Added JWT authentication event logging in `Program.cs`:
```csharp
options.Events = new JwtBearerEvents
{
    OnAuthenticationFailed = context =>
    {
        Console.WriteLine($"Authentication failed: {context.Exception.Message}");
        return Task.CompletedTask;
    },
    OnTokenValidated = context =>
    {
        Console.WriteLine("Token validated successfully");
        return Task.CompletedTask;
    }
};
```

**Check server console** to see:
- "Authentication failed: [reason]" if token is invalid
- "Token validated successfully" if token is good

### 3. ✅ Improved Date Picker Calendar UI

Enhanced the calendar picker icon:
- **Larger clickable area** for the calendar icon
- **Hover effect** - Icon highlights when you hover
- **Always visible** - Calendar icon is always shown
- **Better padding** - More space for easy clicking

```css
input[type="date"]::-webkit-calendar-picker-indicator {
    cursor: pointer;
    opacity: 0.8;
    padding: 8px;  /* Larger clickable area */
}

input[type="date"]::-webkit-calendar-picker-indicator:hover {
    opacity: 1;
    background-color: rgba(99, 102, 241, 0.1);  /* Purple highlight */
    border-radius: 6px;
}
```

## How to Troubleshoot 401 Error

### Step 1: Check Browser Console
Open DevTools (F12) and look for these logs:
```
User token: exists
Creating task with data: {...}
Response status: 401
```

### Step 2: Check Server Console
Look for:
```
Authentication failed: [error message]
```

### Step 3: Common Solutions

#### Solution A: Logout & Login Again
1. Click "Logout"
2. Login again
3. Try creating a task

#### Solution B: Clear Browser Storage
```javascript
// Open browser console and run:
localStorage.clear();
```
Then refresh and login again.

#### Solution C: Restart Development Server
If the server restarted, you MUST logout and login again because:
- Server generates a new JWT secret on startup
- Old tokens are no longer valid with the new secret

## How to Use the Date Picker

### Current Behavior:
1. Click anywhere on the date input field
2. A calendar popup should appear
3. Click on a date to select it
4. Date appears in format: MM/DD/YYYY

### Enhanced Features:
- ✅ Calendar icon is always visible
- ✅ Icon highlights on hover (purple tint)
- ✅ Larger clickable area
- ✅ Cannot select past dates
- ✅ Defaults to today's date

## Testing

### Test 401 Fix:
1. Login to the app
2. Restart the server
3. Try to create a task
4. Should see: "Session expired. Please login again."
5. Should automatically logout
6. Login again - should work now

### Test Date Picker:
1. Look at the date input field
2. You should see a calendar icon on the right
3. Hover over it - should highlight
4. Click anywhere in the field - calendar should open
5. Try to select a past date - should be disabled
6. Select today or future date - should work

## Production Fix

For production, set a permanent JWT secret in `appsettings.json`:

```json
{
  "Jwt": {
    "Secret": "your-super-secure-secret-key-at-least-32-characters-long",
    "Issuer": "LemonTodoAPI",
    "Audience": "LemonTodoClient"
  }
}
```

This way the secret doesn't change on server restart.

## Debug Commands

### Check if token exists in browser:
```javascript
console.log('Token:', localStorage.getItem('token'));
```

### Check token expiration (decode JWT):
```javascript
const token = localStorage.getItem('token');
const payload = JSON.parse(atob(token.split('.')[1]));
console.log('Expires:', new Date(payload.exp * 1000));
```

### Test API call manually:
```javascript
fetch('/api/tasks', {
    method: 'POST',
    headers: {
        'Authorization': `Bearer ${localStorage.getItem('token')}`,
        'Content-Type': 'application/json'
    },
    body: JSON.stringify({
        title: 'Test Task',
        priority: 1,
        dueDate: new Date().toISOString().split('T')[0]
    })
})
.then(r => console.log('Status:', r.status))
.catch(e => console.error(e));
```

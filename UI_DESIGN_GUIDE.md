# ?? UI Design Guide & Component Library

## ?? Color Palette

### Primary Colors
```
Dark Background:   #1a1a2e
Card Background:   #16213e
Darker Blue:       #0f3460
Accent Red:        #e94560
Light Gray:        #a0a0b0
Secondary Gray:    #3a3a4e
White Text:        #ffffff
```

### Usage
- **#1a1a2e** - Main window background
- **#16213e** - Card/tile backgrounds, input fields
- **#0f3460** - Headers, hover states
- **#e94560** - Accent color, active states, highlights
- **#a0a0b0** - Secondary text, labels
- **#3a3a4e** - Borders, dividers

---

## ?? Layout Guidelines

### Spacing
- **Padding (small)**: 10
- **Padding (medium)**: 20
- **Padding (large)**: 40
- **Margin**: 10-15 between elements
- **Border Radius**: 8-12

### Typography
```xaml
<TextBlock FontSize="24" FontWeight="Bold"/>  <!-- Titles -->
<TextBlock FontSize="18" FontWeight="SemiBold"/>  <!-- Subtitles -->
<TextBlock FontSize="14"/>  <!-- Body text -->
<TextBlock FontSize="12"/>  <!-- Small text, labels -->
```

---

## ?? Reusable Components

### 1. Modern Button Style
```xaml
<Style x:Key="ModernButton" TargetType="Button">
    <Setter Property="Background" Value="#16213e"/>
    <Setter Property="Foreground" Value="White"/>
    <Setter Property="FontSize" Value="14"/>
    <Setter Property="FontWeight" Value="SemiBold"/>
    <Setter Property="Padding" Value="20,10"/>
    <Setter Property="BorderThickness" Value="0"/>
    <Setter Property="Cursor" Value="Hand"/>
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="Button">
                <Border Background="{TemplateBinding Background}"
                        CornerRadius="8"
                        Padding="{TemplateBinding Padding}">
                    <ContentPresenter HorizontalAlignment="Center" VerticalAlignment="Center"/>
                </Border>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
    <Style.Triggers>
        <Trigger Property="IsMouseOver" Value="True">
            <Setter Property="Background" Value="#0f3460"/>
        </Trigger>
        <Trigger Property="IsEnabled" Value="False">
            <Setter Property="Background" Value="#3a3a4e"/>
            <Setter Property="Foreground" Value="#7a7a8e"/>
        </Trigger>
    </Style.Triggers>
</Style>
```

### 2. Modern TextBox Style
```xaml
<Style x:Key="ModernTextBox" TargetType="TextBox">
    <Setter Property="Background" Value="#16213e"/>
    <Setter Property="Foreground" Value="White"/>
    <Setter Property="FontSize" Value="14"/>
    <Setter Property="Padding" Value="12,10"/>
    <Setter Property="BorderBrush" Value="#3a3a4e"/>
    <Setter Property="BorderThickness" Value="2"/>
    <Setter Property="Template">
        <Setter.Value>
            <ControlTemplate TargetType="TextBox">
                <Border Background="{TemplateBinding Background}"
                        BorderBrush="{TemplateBinding BorderBrush}"
                        BorderThickness="{TemplateBinding BorderThickness}"
                        CornerRadius="8">
                    <ScrollViewer x:Name="PART_ContentHost" 
                                 Padding="{TemplateBinding Padding}"/>
                </Border>
            </ControlTemplate>
        </Setter.Value>
    </Setter>
    <Style.Triggers>
        <Trigger Property="IsFocused" Value="True">
            <Setter Property="BorderBrush" Value="#e94560"/>
        </Trigger>
    </Style.Triggers>
</Style>
```

### 3. Card/Tile Style
```xaml
<Style x:Key="CardBorder" TargetType="Border">
    <Setter Property="Background" Value="#16213e"/>
    <Setter Property="BorderBrush" Value="#3a3a4e"/>
    <Setter Property="BorderThickness" Value="2"/>
    <Setter Property="CornerRadius" Value="12"/>
    <Setter Property="Padding" Value="20"/>
    <Style.Triggers>
        <Trigger Property="IsMouseOver" Value="True">
            <Setter Property="BorderBrush" Value="#e94560"/>
        </Trigger>
    </Style.Triggers>
</Style>
```

### 4. Icon Button Style
```xaml
<Style x:Key="IconButton" TargetType="Button">
    <Setter Property="Background" Value="Transparent"/>
    <Setter Property="Foreground" Value="#a0a0b0"/>
    <Setter Property="BorderThickness" Value="0"/>
    <Setter Property="FontSize" Value="20"/>
    <Setter Property="Cursor" Value="Hand"/>
    <Setter Property="Padding" Value="10"/>
    <Style.Triggers>
        <Trigger Property="IsMouseOver" Value="True">
            <Setter Property="Foreground" Value="#e94560"/>
        </Trigger>
    </Style.Triggers>
</Style>
```

### 5. DataGrid Style
```xaml
<Style x:Key="ModernDataGrid" TargetType="DataGrid">
    <Setter Property="Background" Value="#16213e"/>
    <Setter Property="Foreground" Value="White"/>
    <Setter Property="BorderBrush" Value="#3a3a4e"/>
    <Setter Property="BorderThickness" Value="2"/>
    <Setter Property="RowBackground" Value="#16213e"/>
    <Setter Property="AlternatingRowBackground" Value="#1a1f33"/>
    <Setter Property="HeadersVisibility" Value="Column"/>
    <Setter Property="GridLinesVisibility" Value="None"/>
    <Setter Property="AutoGenerateColumns" Value="False"/>
    <Setter Property="CanUserAddRows" Value="False"/>
    <Setter Property="SelectionMode" Value="Single"/>
</Style>
```

---

## ?? Window Templates

### Standard Window Template
```xaml
<Window x:Class="..."
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Window Title" 
        Height="600" 
        Width="800"
        WindowStartupLocation="CenterScreen"
        Background="#1a1a2e">
    
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/> <!-- Header -->
            <RowDefinition Height="*"/>    <!-- Content -->
            <RowDefinition Height="Auto"/> <!-- Footer (optional) -->
        </Grid.RowDefinitions>
        
        <!-- Header -->
        <Border Grid.Row="0" Background="#0f3460" Padding="20">
            <TextBlock Text="Header Title" 
                      FontSize="20" 
                      FontWeight="Bold" 
                      Foreground="White"/>
        </Border>
        
        <!-- Content -->
        <ScrollViewer Grid.Row="1" 
                     VerticalScrollBarVisibility="Auto"
                     Padding="20">
            <!-- Content here -->
        </ScrollViewer>
        
        <!-- Footer (optional) -->
        <Border Grid.Row="2" 
               Background="#0f3460" 
               Padding="20"
               BorderBrush="#3a3a4e"
               BorderThickness="0,2,0,0">
            <StackPanel Orientation="Horizontal" HorizontalAlignment="Right">
                <Button Content="Cancel" Style="{StaticResource ModernButton}" Margin="0,0,10,0"/>
                <Button Content="Save" Style="{StaticResource ModernButton}"/>
            </StackPanel>
        </Border>
    </Grid>
</Window>
```

### Dialog Window Template
```xaml
<Window x:Class="..."
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="Dialog Title" 
        Height="400" 
        Width="500"
        WindowStartupLocation="CenterOwner"
        ResizeMode="NoResize"
        Background="#1a1a2e"
        WindowStyle="None"
        AllowsTransparency="True">
    
    <Border BorderBrush="#e94560" 
           BorderThickness="2" 
           CornerRadius="12"
           Background="#1a1a2e">
        <Grid>
            <!-- Dialog content -->
        </Grid>
    </Border>
</Window>
```

---

## ?? UI Component Specifications

### Session Tile (Dashboard)
**Size**: 250x200
**Content**:
```
???????????????????????????
?     Table #1            ? ? 20pt Bold
?                         ?
?     Single              ? ? 14pt Red (#e94560)
?                         ?
?   Ali vs Usman          ? ? 12pt Gray
?                         ?
?     00:15:32            ? ? 18pt Bold White
?                         ?
?   Frames: 2             ? ? 12pt Gray
???????????????????????????
```

### Billing Card
**Size**: 400x500
```
??????????????????????????????
?    Billing Summary         ? ? Header
??????????????????????????????
? Base Rate:      Rs 500     ?
? Time: 25 min                ?
? Overtime: 5 min            ?
? Overtime Charge: Rs 25     ?
? ?????????????????????????  ?
? Subtotal:       Rs 525     ?
? Discount:      -Rs 0   [??]?
? ???????????????????????    ?
? Total:          Rs 525     ?
??????????????????????????????
? Payer Mode:                ?
? ? Loser Pays               ?
? ? Split Equally            ?
? ? Custom                   ?
??????????????????????????????
? Payment Method:            ?
? [  Cash  ?]                ?
??????????????????????????????
? [?? Pay Now] [?? Credit]   ?
??????????????????????????????
```

### Customer Card
```
??????????????????????????????
?  ?? Ali Raza               ?
?  ?? 0300-1234567           ?
??????????????????????????????
?  Total Games:      15      ?
?  Total Spent:   Rs 7,500   ?
?  Outstanding:   Rs 1,200   ?
?  Win Rate:      60%        ?
??????????????????????????????
? [View History] [?? Payment]?
??????????????????????????????
```

---

## ??? Icon Library (Using Unicode)

```
?? - Snooker/Billiards
?? - Timer
?? - User/Customer
?? - Money/Payment
?? - Reports
?? - Refresh
?? - Edit
??? - Delete
? - Success
? - Cancel/Error
?? - Note/Credit
?? - Search
?? - Analytics
?? - Phone
?? - Email
??? - Print
?? - Settings
?? - Winner/Trophy
?? - Target/Goal
```

---

## ?? Form Layout Best Practices

### Label-Input Pairs
```xaml
<StackPanel Margin="0,0,0,15">
    <TextBlock Text="Full Name" 
              Foreground="#a0a0b0" 
              FontSize="13" 
              Margin="0,0,0,5"/>
    <TextBox Style="{StaticResource ModernTextBox}"
            Text="{Binding FullName}"/>
</StackPanel>
```

### Button Groups
```xaml
<StackPanel Orientation="Horizontal" HorizontalAlignment="Right" Margin="0,20,0,0">
    <Button Content="Cancel" 
           Style="{StaticResource ModernButton}" 
           Margin="0,0,10,0"
           Width="100"/>
    <Button Content="Save" 
           Style="{StaticResource ModernButton}"
           Width="100"/>
</StackPanel>
```

### Radio Button Groups
```xaml
<StackPanel>
    <TextBlock Text="Payer Mode" Foreground="White" FontSize="14" Margin="0,0,0,10"/>
    <RadioButton Content="Loser Pays" 
                Foreground="White" 
                IsChecked="True"
                Margin="0,0,0,5"/>
    <RadioButton Content="Split Equally" 
                Foreground="White"
                Margin="0,0,0,5"/>
    <RadioButton Content="Custom" 
                Foreground="White"/>
</StackPanel>
```

---

## ?? Animation Suggestions

### Fade In
```xaml
<Window.Resources>
    <Storyboard x:Key="FadeIn">
        <DoubleAnimation Storyboard.TargetProperty="Opacity"
                        From="0" To="1" Duration="0:0:0.3"/>
    </Storyboard>
</Window.Resources>
```

### Slide In
```xaml
<Storyboard x:Key="SlideIn">
    <DoubleAnimation Storyboard.TargetProperty="(UIElement.RenderTransform).(TranslateTransform.X)"
                    From="100" To="0" Duration="0:0:0.3">
        <DoubleAnimation.EasingFunction>
            <QuadraticEase EasingMode="EaseOut"/>
        </DoubleAnimation.EasingFunction>
    </DoubleAnimation>
</Storyboard>
```

---

## ??? Interaction States

### Button States
- **Normal**: Background #16213e
- **Hover**: Background #0f3460, Scale 1.02
- **Pressed**: Background #0a2440, Scale 0.98
- **Disabled**: Background #3a3a4e, Foreground #7a7a8e

### Input States
- **Normal**: BorderBrush #3a3a4e
- **Focus**: BorderBrush #e94560
- **Error**: BorderBrush #ff4444
- **Success**: BorderBrush #44ff44

---

## ?? Responsive Breakpoints

### Window Sizes
- **Minimum**: 1024x768
- **Recommended**: 1280x720
- **Large**: 1920x1080

### Tile Responsive Grid
```xaml
<WrapPanel>
    <!-- Tiles adjust automatically based on window width -->
    <!-- Each tile: 250px + 20px margin = 270px per tile -->
</WrapPanel>
```

---

## ?? Usage Examples

### Creating a New Window
1. Copy standard window template
2. Replace title and dimensions
3. Add your content in content area
4. Wire up ViewModel in code-behind
5. Register in DI container

### Creating a New Card
1. Use CardBorder style
2. Add Grid for layout
3. Use typography guidelines
4. Maintain spacing consistency

### Adding a New Form
1. Use StackPanel for vertical layout
2. Label-input pairs with 15px margin
3. Button group at bottom right
4. Use validation converters

---

## ?? Design Principles

1. **Consistency** - Use same colors, fonts, spacing throughout
2. **Clarity** - Clear labels, obvious actions
3. **Feedback** - Show loading states, success/error messages
4. **Efficiency** - Minimize clicks, provide shortcuts
5. **Beauty** - Modern, clean, professional appearance

---

**?? Follow these guidelines to maintain consistent, beautiful UI throughout the application!**

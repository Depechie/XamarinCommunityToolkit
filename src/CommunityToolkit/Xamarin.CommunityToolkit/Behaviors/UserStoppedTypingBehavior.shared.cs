﻿using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.CommunityToolkit.Behaviors.Internals;
using Xamarin.Forms;

namespace Xamarin.CommunityToolkit.Behaviors
{
	/// <summary>
	/// The <see cref="UserStoppedTypingBehavior"/> is a behavior that allows the user to trigger an action when a user has stopped data input any <see cref="InputView"/> derivate like <see cref="Entry"/> or <see cref="SearchBar"/>. Examples of its usage include triggering a search when a user has stopped entering their search query.
	/// </summary>
	public class UserStoppedTypingBehavior : BaseBehavior<InputView>
	{
		/// <summary>
		/// Backing BindableProperty for the <see cref="Command"/> property.
		/// </summary>
		public static readonly BindableProperty CommandProperty
			= BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(UserStoppedTypingBehavior));

		/// <summary>
		/// Backing BindableProperty for the <see cref="StoppedTypingTimeThreshold"/> property.
		/// </summary>
		public static readonly BindableProperty StoppedTypingTimeThresholdProperty
			= BindableProperty.Create(nameof(StoppedTypingTimeThreshold), typeof(int), typeof(UserStoppedTypingBehavior), 1000);

		/// <summary>
		/// Backing BindableProperty for the <see cref="MinimumLengthThreshold"/> property.
		/// </summary>
		public static readonly BindableProperty MinimumLengthThresholdProperty
			= BindableProperty.Create(nameof(MinimumLengthThreshold), typeof(int), typeof(UserStoppedTypingBehavior), 0);

		/// <summary>
		/// Backing BindableProperty for the <see cref="ShouldDismissKeyboardAutomatically"/> property.
		/// </summary>
		public static readonly BindableProperty ShouldDismissKeyboardAutomaticallyProperty
			= BindableProperty.Create(nameof(ShouldDismissKeyboardAutomatically), typeof(bool), typeof(UserStoppedTypingBehavior), false);

		CancellationTokenSource tokenSource;

		/// <summary>
		/// Command that is triggered when the <see cref="StoppedTypingTimeThreshold" /> is reached. When <see cref="MinimumLengthThreshold"/> is set, it's only triggered when both conditions are met. This is a bindable property.
		/// </summary>
		public ICommand Command
		{
			get => (ICommand)GetValue(CommandProperty);
			set => SetValue(CommandProperty, value);
		}

		/// <summary>
		/// The time of inactivity in milliseconds after which <see cref="Command"/> will be executed. If <see cref="MinimumLengthThreshold"/> is also set, the condition there also needs to be met. This is a bindable property.
		/// </summary>
		public int StoppedTypingTimeThreshold
		{
			get => (int)GetValue(StoppedTypingTimeThresholdProperty);
			set => SetValue(StoppedTypingTimeThresholdProperty, value);
		}

		/// <summary>
		/// The minimum length of the input value required before <see cref="Command"/> will be executed but only after <see cref="StoppedTypingTimeThreshold"/> has passed. This is a bindable property.
		/// </summary>
		public int MinimumLengthThreshold
		{
			get => (int)GetValue(MinimumLengthThresholdProperty);
			set => SetValue(MinimumLengthThresholdProperty, value);
		}

		/// <summary>
		/// Indicates whether or not the keyboard should be dismissed automatically after the user stopped typing. This is a bindable property.
		/// </summary>
		public bool ShouldDismissKeyboardAutomatically
		{
			get => (bool)GetValue(ShouldDismissKeyboardAutomaticallyProperty);
			set => SetValue(ShouldDismissKeyboardAutomaticallyProperty, value);
		}

		protected override void OnViewPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnViewPropertyChanged(sender, e);
			if (e.PropertyName == InputView.TextProperty.PropertyName)
				OnTextPropertyChanged();
		}

		void OnTextPropertyChanged()
		{
			if (tokenSource != null)
			{
				tokenSource.Cancel();
				tokenSource.Dispose();
			}
			tokenSource = new CancellationTokenSource();

			_ = Task.Delay(StoppedTypingTimeThreshold, tokenSource.Token)
				.ContinueWith(task =>
				{
					if (task.Status == TaskStatus.Canceled ||
						View.Text.Length < MinimumLengthThreshold)
						return;

					if (ShouldDismissKeyboardAutomatically)
						Device.BeginInvokeOnMainThread(View.Unfocus);

					if (Command?.CanExecute(View.Text) ?? false)
						Command.Execute(View.Text);
				});
		}
	}
}
use iced::executor;
use iced::widget::{checkbox, column, container};
use iced::{Application, Command, Element, Length, Settings, Theme};

pub fn main() -> iced::Result {
    Example::run(Settings::default())
}

#[derive(Default)]
struct Example {
    default_checkbox: bool,
}

#[derive(Debug, Clone, Copy)]
enum Message {
    DefaultChecked(bool),
}

impl Application for Example {
    type Message = Message;
    type Flags = ();
    type Executor = executor::Default;
    type Theme = Theme;

    fn new(_flags: Self::Flags) -> (Self, Command<Message>) {
        (Self::default(), Command::none())
    }

    fn title(&self) -> String {
        String::from("Checkbox - Iced")
    }

    fn update(&mut self, message: Message) -> Command<Message> {
        match message {
            Message::DefaultChecked(value) => self.default_checkbox = value,
        }

        Command::none()
    }

    fn view(&self) -> Element<Message> {
        let default_checkbox = checkbox("Default", self.default_checkbox, Message::DefaultChecked);

        let content = column![default_checkbox].spacing(22);

        container(content)
            .width(Length::Fill)
            .height(Length::Fill)
            .center_x()
            .center_y()
            .into()
    }
}

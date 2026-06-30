from __future__ import annotations

from email.message import EmailMessage
import smtplib

from pulse_console.config import Settings


class EmailClient:
    def __init__(self, settings: Settings) -> None:
        self._settings = settings

    def send_html_email(
        self,
        *,
        subject: str,
        html_body: str,
        to_addresses: list[str],
        cc_addresses: list[str] | None = None,
    ) -> None:
        if not to_addresses:
            raise ValueError("At least one recipient is required.")

        message = EmailMessage()
        message["Subject"] = subject
        message["From"] = f"{self._settings.smtp_from_display} <{self._settings.smtp_from_address}>"
        message["To"] = "; ".join(to_addresses)
        if cc_addresses:
            message["Cc"] = "; ".join(cc_addresses)
        message.set_content("This message contains HTML content. Use an HTML-capable email client.")
        message.add_alternative(html_body, subtype="html")

        recipients = list(to_addresses)
        if cc_addresses:
            recipients.extend(cc_addresses)

        if self._settings.smtp_use_ssl:
            smtp: smtplib.SMTP | smtplib.SMTP_SSL = smtplib.SMTP_SSL(
                self._settings.smtp_host,
                self._settings.smtp_port,
                timeout=30,
            )
        else:
            smtp = smtplib.SMTP(self._settings.smtp_host, self._settings.smtp_port, timeout=30)

        with smtp:
            if not self._settings.smtp_use_ssl:
                smtp.ehlo()
                if self._settings.smtp_use_tls:
                    smtp.starttls()
                    smtp.ehlo()

            smtp.send_message(message, to_addrs=recipients)

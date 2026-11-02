# GoldEx responsive dialog motion
- MeetFlow's src/Client/MeetFlow.Client/Components/MfDialog.razor.css and native modal lifecycle are the reference.
- Mobile modal should fade and rise 18px with minimal scale rather than sliding 100% of a tall form. Avoid mobile backdrop-filter blur as it repaints a viewport; retain dim overlay.
- Update viewport geometry for actual resize/keyboard changes only. Never use per-scroll style writes. Preserve focus, Escape, non-dismissable backdrop, native showModal, stable overlay providers and nested forms.
- Run node --test scripts/tests/responsive-dialog.test.mjs and full JS suite. Authenticated browser visual proof is separate from source tests.

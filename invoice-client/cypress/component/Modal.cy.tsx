import { Modal } from '../../src/components/Modal';

describe('Modal', () => {
  it('renders nothing when closed', () => {
    cy.mount(
      <Modal open={false} onClose={() => {}} title="Hidden Modal Title">
        <p>Hidden modal content</p>
      </Modal>
    );

    cy.contains('Hidden Modal Title').should('not.exist');
    cy.contains('Hidden modal content').should('not.exist');
  });

  it('renders title and children when open', () => {
    cy.mount(
      <Modal open={true} onClose={() => {}} title="My Modal">
        <p>Modal body content</p>
      </Modal>
    );

    cy.contains('My Modal');
    cy.contains('Modal body content');
  });

  it('calls onClose when X button is clicked', () => {
    const onClose = cy.stub().as('close');

    cy.mount(
      <Modal open={true} onClose={onClose} title="Closeable">
        <p>Content</p>
      </Modal>
    );

    // Click the X button (the button inside the header)
    cy.get('button').click();
    cy.get('@close').should('have.been.calledOnce');
  });

  it('applies wide class when wide prop is true', () => {
    cy.mount(
      <Modal open={true} onClose={() => {}} title="Wide" wide>
        <p>Wide content</p>
      </Modal>
    );

    cy.get('.max-w-4xl').should('exist');
  });

  it('applies narrow class by default', () => {
    cy.mount(
      <Modal open={true} onClose={() => {}} title="Narrow">
        <p>Narrow content</p>
      </Modal>
    );

    cy.get('.max-w-lg').should('exist');
  });
});
